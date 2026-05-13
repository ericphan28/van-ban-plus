using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AIVanBan.Core.Data;
using AIVanBan.Core.Models;
using LiteDB;

// Alias để tránh ambiguous giữa System.Text.Json.JsonSerializer và LiteDB.JsonSerializer
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace AIVanBan.Core.Services
{
    /// <summary>
    /// Cloud Sync Service — đồng bộ dữ liệu local (LiteDB) ↔ cloud (Supabase).
    /// 
    /// Architecture: Local-first + Cloud sync
    /// - App luôn đọc/ghi local DB trước (offline-first)
    /// - Background worker sync với cloud định kỳ
    /// - Conflict resolution: Last-Write-Wins (auto) hoặc Manual
    /// </summary>
    public class CloudSyncService : IDisposable
    {
        private readonly CloudApiClient _api;
        private readonly ILiteDatabase _db;
        private readonly ILiteCollection<SyncMetadata> _syncCol;
        private Timer? _autoSyncTimer;
        private bool _isSyncing;
        private readonly object _syncLock = new();
        private CancellationTokenSource? _cts;

        // Events cho UI binding
        public event Action<string>? OnSyncStatusChanged;
        public event Action<SyncResult>? OnSyncCompleted;
        public event Action<string>? OnSyncError;
        public event Action<int>? OnSyncProgress;

        public bool IsSyncing => _isSyncing;
        public DateTime? LastSyncTime => GetSyncSettings().LastSyncTimestamp;

        public CloudSyncService()
        {
            _api = new CloudApiClient();
            _db = DatabaseFactory.GetDatabase();
            _syncCol = _db.GetCollection<SyncMetadata>("sync_metadata");
            _syncCol.EnsureIndex(x => x.LocalId);
            _syncCol.EnsureIndex(x => x.EntityType);
            _syncCol.EnsureIndex(x => x.State);
        }

        // ==================== Khởi tạo & Cấu hình ====================

        /// <summary>
        /// Khởi tạo service với API credentials.
        /// Gọi khi app start hoặc khi user đăng nhập.
        /// </summary>
        public void Initialize()
        {
            var settings = AppSettingsService.GetSettings();
            if (string.IsNullOrEmpty(settings.VanBanPlusApiUrl) || string.IsNullOrEmpty(settings.VanBanPlusApiKey))
            {
                OnSyncError?.Invoke("Chưa cấu hình VanBanPlus API. Vui lòng đăng nhập.");
                return;
            }

            _api.Configure(settings.VanBanPlusApiUrl, settings.VanBanPlusApiKey);

            // Tạo device ID nếu chưa có
            var syncSettings = GetSyncSettings();
            if (string.IsNullOrEmpty(syncSettings.DeviceId))
            {
                syncSettings.DeviceId = Guid.NewGuid().ToString("N")[..16];
                syncSettings.DeviceName = Environment.MachineName;
                SaveSyncSettings(syncSettings);
            }
        }

        /// <summary>
        /// Bắt đầu auto sync.
        /// </summary>
        public void StartAutoSync()
        {
            var syncSettings = GetSyncSettings();
            if (!syncSettings.Enabled || !syncSettings.AutoSyncEnabled) return;

            var interval = TimeSpan.FromMinutes(Math.Max(syncSettings.AutoSyncIntervalMinutes, 1));
            
            _autoSyncTimer?.Dispose();
            _autoSyncTimer = new Timer(
                async _ => await RunSync(),
                null,
                syncSettings.SyncOnStartup ? TimeSpan.Zero : interval,
                interval
            );

            OnSyncStatusChanged?.Invoke("Cloud Sync đã bật — đồng bộ tự động");
        }

        /// <summary>
        /// Dừng auto sync.
        /// </summary>
        public void StopAutoSync()
        {
            _autoSyncTimer?.Dispose();
            _autoSyncTimer = null;
            _cts?.Cancel();
            OnSyncStatusChanged?.Invoke("Cloud Sync đã tắt");
        }

        /// <summary>
        /// Retry một async operation với exponential backoff.
        /// Retry khi gặp lỗi mạng (HttpRequestException, TaskCanceledException timeout).
        /// </summary>
        private async Task<ApiResponse<T>> RetryAsync<T>(
            Func<Task<ApiResponse<T>>> operation,
            int maxRetries = 3,
            string label = "")
        {
            ApiResponse<T>? lastResult = null;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                if (attempt > 0)
                {
                    // Exponential backoff: 1s, 2s, 4s
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    OnSyncStatusChanged?.Invoke($"⏳ Retry {label} ({attempt}/{maxRetries}) sau {delay.TotalSeconds:F0}s...");
                    await Task.Delay(delay, _cts?.Token ?? CancellationToken.None);
                }

                try
                {
                    lastResult = await operation();
                    if (lastResult.Success) return lastResult;

                    // Nếu lỗi 4xx (client error) → không retry
                    if (lastResult.Message?.Contains("401") == true ||
                        lastResult.Message?.Contains("403") == true ||
                        lastResult.Message?.Contains("400") == true)
                    {
                        return lastResult;
                    }
                }
                catch (HttpRequestException) when (attempt < maxRetries)
                {
                    // Network error → retry
                    continue;
                }
                catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested && attempt < maxRetries)
                {
                    // Timeout (not user cancellation) → retry
                    continue;
                }
            }

            return lastResult ?? new ApiResponse<T>
            {
                Success = false,
                Message = $"{label}: Thất bại sau {maxRetries} lần thử"
            };
        }

        // ==================== Sync Operations ====================

        /// <summary>
        /// Chạy sync thủ công (hoặc từ auto timer).
        /// Có retry tự động với exponential backoff khi gặp lỗi mạng.
        /// </summary>
        public async Task<SyncResult?> RunSync()
        {
            lock (_syncLock)
            {
                if (_isSyncing) return null;
                _isSyncing = true;
            }

            _cts = new CancellationTokenSource();
            OnSyncStatusChanged?.Invoke("Đang đồng bộ...");

            try
            {
                var syncSettings = GetSyncSettings();
                var deviceId = syncSettings.DeviceId;
                var since = syncSettings.LastSyncTimestamp ?? new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                // 1. Thu thập local changes cần push
                var pendingChanges = GetPendingChanges(syncSettings);
                OnSyncProgress?.Invoke(10);

                // 2. Push changes lên cloud (với retry)
                SyncResult? result = null;
                if (pendingChanges.Count > 0)
                {
                    // Chia thành batch nếu có nhiều items (mỗi batch tối đa 50)
                    const int batchSize = 50;
                    var totalPushed = 0;
                    var batches = pendingChanges
                        .Select((item, idx) => new { item, idx })
                        .GroupBy(x => x.idx / batchSize)
                        .Select(g => g.Select(x => x.item).ToList())
                        .ToList();

                    var batchIdx = 0;
                    foreach (var batch in batches)
                    {
                        batchIdx++;
                        OnSyncStatusChanged?.Invoke($"Đang đẩy lên cloud... (batch {batchIdx}/{batches.Count})");

                        var pushResponse = await RetryAsync(
                            () => _api.SyncPush(deviceId, syncSettings.DeviceName, batch, _cts!.Token),
                            maxRetries: 3, label: "Push");

                        if (pushResponse.Success && pushResponse.Data != null)
                        {
                            totalPushed += pushResponse.Data.ItemsPushed;
                            MarkAsSynced(batch.Select(e => e.LocalId).ToList());
                        }
                        else
                        {
                            OnSyncError?.Invoke($"Push batch {batchIdx} thất bại: {pushResponse.Message}");
                        }

                        // Cập nhật progress dựa trên batch
                        var pushProgress = 10 + (int)(40.0 * batchIdx / batches.Count);
                        OnSyncProgress?.Invoke(pushProgress);
                    }

                    result = new SyncResult
                    {
                        Status = "completed",
                        ItemsPushed = totalPushed,
                        SyncTimestamp = DateTime.UtcNow.ToString("o"),
                    };
                }
                OnSyncProgress?.Invoke(50);

                // 3. Pull changes từ cloud (với retry)
                OnSyncStatusChanged?.Invoke("Đang kéo dữ liệu từ cloud...");
                var pullResponse = await RetryAsync(
                    () => _api.SyncPull(deviceId, since, null, _cts!.Token),
                    maxRetries: 3, label: "Pull");
                OnSyncProgress?.Invoke(75);

                if (pullResponse.Success && pullResponse.Data != null)
                {
                    var pullData = pullResponse.Data;
                    ApplyCloudChanges(pullData.Changes);

                    // Cập nhật timestamp
                    syncSettings.LastSyncTimestamp = DateTime.TryParse(pullData.SyncTimestamp, out var ts)
                        ? ts : DateTime.UtcNow;
                    SaveSyncSettings(syncSettings);

                    if (result == null)
                    {
                        result = new SyncResult
                        {
                            Status = "completed",
                            ItemsPulled = pullData.Changes.Count,
                            SyncTimestamp = pullData.SyncTimestamp,
                        };
                    }
                    else
                    {
                        result.ItemsPulled = pullData.Changes.Count;
                    }
                }

                OnSyncProgress?.Invoke(100);
                OnSyncStatusChanged?.Invoke(
                    $"✅ Đồng bộ xong — Push: {result?.ItemsPushed ?? 0}, Pull: {result?.ItemsPulled ?? 0}");
                OnSyncCompleted?.Invoke(result ?? new SyncResult { Status = "completed" });

                // Invalidate SyncTracker cache
                SyncTracker.InvalidateCache();

                return result;
            }
            catch (OperationCanceledException)
            {
                OnSyncStatusChanged?.Invoke("Đồng bộ đã hủy");
                return null;
            }
            catch (Exception ex)
            {
                OnSyncError?.Invoke($"Lỗi đồng bộ: {ex.Message}");
                return null;
            }
            finally
            {
                _isSyncing = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        // ==================== Track Changes ====================

        /// <summary>
        /// Đánh dấu entity đã thay đổi trên local (gọi sau mỗi CRUD operation).
        /// </summary>
        public void MarkLocalChanged(string localId, string entityType)
        {
            var meta = _syncCol.FindOne(x => x.LocalId == localId && x.EntityType == entityType);
            if (meta == null)
            {
                meta = new SyncMetadata
                {
                    LocalId = localId,
                    EntityType = entityType,
                    State = SyncState.LocalOnly,
                    LocalUpdatedAt = DateTime.UtcNow,
                };
                _syncCol.Insert(meta);
            }
            else
            {
                if (meta.State == SyncState.Synced)
                    meta.State = SyncState.LocalModified;
                meta.LocalUpdatedAt = DateTime.UtcNow;
                _syncCol.Update(meta);
            }
        }

        /// <summary>
        /// Đánh dấu entity đã xóa trên local.
        /// </summary>
        public void MarkLocalDeleted(string localId, string entityType)
        {
            var meta = _syncCol.FindOne(x => x.LocalId == localId && x.EntityType == entityType);
            if (meta != null)
            {
                meta.State = SyncState.LocalDeleted;
                meta.LocalUpdatedAt = DateTime.UtcNow;
                _syncCol.Update(meta);
            }
        }

        // ==================== Internal Logic ====================

        /// <summary>
        /// Thu thập các entities cần push lên cloud.
        /// </summary>
        private List<SyncEntity> GetPendingChanges(CloudSyncSettings syncSettings)
        {
            var pending = new List<SyncEntity>();

            var modifiedMeta = _syncCol.FindAll()
                .Where(m => m.State == SyncState.LocalOnly || m.State == SyncState.LocalModified || m.State == SyncState.LocalDeleted)
                .ToList();

            foreach (var meta in modifiedMeta)
            {
                // Kiểm tra entity type có được sync không
                if (!ShouldSyncEntityType(meta.EntityType, syncSettings)) continue;

                if (meta.State == SyncState.LocalDeleted)
                {
                    pending.Add(new SyncEntity
                    {
                        EntityType = meta.EntityType,
                        Action = "delete",
                        LocalId = meta.LocalId,
                        LocalUpdatedAt = meta.LocalUpdatedAt.ToString("o"),
                    });
                    continue;
                }

                // Lấy data từ local DB
                var data = GetLocalEntityData(meta.LocalId, meta.EntityType);
                if (data == null) continue;

                pending.Add(new SyncEntity
                {
                    EntityType = meta.EntityType,
                    Action = "upsert",
                    LocalId = meta.LocalId,
                    LocalUpdatedAt = meta.LocalUpdatedAt.ToString("o"),
                    Data = data,
                });
            }

            return pending;
        }

        /// <summary>
        /// Lấy data entity từ local DB dưới dạng Dictionary.
        /// </summary>
        private Dictionary<string, object?>? GetLocalEntityData(string localId, string entityType)
        {
            try
            {
                switch (entityType)
                {
                    case "document":
                        var doc = _db.GetCollection<Document>("documents").FindById(localId);
                        if (doc == null) return null;
                        return DocumentToDict(doc);

                    case "meeting":
                        var meeting = _db.GetCollection<Meeting>("meetings").FindById(localId);
                        if (meeting == null) return null;
                        return MeetingToDict(meeting);

                    case "template":
                        var template = _db.GetCollection<DocumentTemplate>("templates").FindById(localId);
                        if (template == null) return null;
                        return TemplateToDict(template);

                    case "folder":
                        var folder = _db.GetCollection<Folder>("folders").FindById(localId);
                        if (folder == null) return null;
                        return FolderToDict(folder);

                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Áp dụng changes từ cloud vào local DB.
        /// </summary>
        private void ApplyCloudChanges(List<SyncChange> changes)
        {
            foreach (var change in changes)
            {
                try
                {
                    // Kiểm tra conflict
                    var meta = _syncCol.FindOne(x => x.LocalId == change.LocalId && x.EntityType == change.EntityType);

                    if (meta != null && (meta.State == SyncState.LocalModified || meta.State == SyncState.LocalDeleted))
                    {
                        // Conflict — cloud muốn update nhưng local cũng đã thay đổi
                        meta.State = SyncState.Conflicted;
                        _syncCol.Update(meta);
                        continue; // Skip, cần user resolve
                    }

                    if (change.Action == "delete")
                    {
                        DeleteLocalEntity(change.LocalId, change.EntityType);
                        if (meta != null) _syncCol.Delete(meta.Id);
                        continue;
                    }

                    // Upsert — ghi vào local DB
                    UpsertLocalEntity(change.LocalId, change.EntityType, change.Data);

                    // Cập nhật sync metadata
                    if (meta == null)
                    {
                        meta = new SyncMetadata
                        {
                            LocalId = change.LocalId,
                            EntityType = change.EntityType,
                        };
                        _syncCol.Insert(meta);
                    }

                    meta.CloudId = change.EntityId;
                    meta.State = SyncState.Synced;
                    meta.CloudUpdatedAt = DateTime.TryParse(change.UpdatedAt, out var dt) ? dt : DateTime.UtcNow;
                    meta.LastSyncedAt = DateTime.UtcNow;
                    _syncCol.Update(meta);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[SYNC] Error applying change {change.EntityType}/{change.LocalId}: {ex.Message}");
                }
            }
        }

        private void UpsertLocalEntity(string localId, string entityType, Dictionary<string, object?> data)
        {
            var json = JsonSerializer.Serialize(data);

            switch (entityType)
            {
                case "document":
                    var doc = JsonSerializer.Deserialize<Document>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (doc != null)
                    {
                        doc.Id = localId;
                        _db.GetCollection<Document>("documents").Upsert(doc);
                    }
                    break;

                case "meeting":
                    var meeting = JsonSerializer.Deserialize<Meeting>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (meeting != null)
                    {
                        meeting.Id = localId;
                        _db.GetCollection<Meeting>("meetings").Upsert(meeting);
                    }
                    break;

                case "template":
                    var template = JsonSerializer.Deserialize<DocumentTemplate>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (template != null)
                    {
                        template.Id = localId;
                        _db.GetCollection<DocumentTemplate>("templates").Upsert(template);
                    }
                    break;

                case "folder":
                    var folder = JsonSerializer.Deserialize<Folder>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (folder != null)
                    {
                        folder.Id = localId;
                        _db.GetCollection<Folder>("folders").Upsert(folder);
                    }
                    break;
            }
        }

        private void DeleteLocalEntity(string localId, string entityType)
        {
            switch (entityType)
            {
                case "document":
                    _db.GetCollection<Document>("documents").Delete(localId);
                    break;
                case "meeting":
                    _db.GetCollection<Meeting>("meetings").Delete(localId);
                    break;
                case "template":
                    _db.GetCollection<DocumentTemplate>("templates").Delete(localId);
                    break;
                case "folder":
                    _db.GetCollection<Folder>("folders").Delete(localId);
                    break;
            }
        }

        private void MarkAsSynced(List<string> localIds)
        {
            foreach (var id in localIds)
            {
                var meta = _syncCol.FindAll().Where(m => m.LocalId == id).FirstOrDefault();
                if (meta != null)
                {
                    if (meta.State == SyncState.LocalDeleted)
                    {
                        _syncCol.Delete(meta.Id);
                    }
                    else
                    {
                        meta.State = SyncState.Synced;
                        meta.LastSyncedAt = DateTime.UtcNow;
                        meta.FailCount = 0;
                        meta.LastError = null;
                        _syncCol.Update(meta);
                    }
                }
            }
        }

        private bool ShouldSyncEntityType(string entityType, CloudSyncSettings settings)
        {
            return entityType switch
            {
                "document" => settings.SyncDocuments,
                "meeting" => settings.SyncMeetings,
                "template" => settings.SyncTemplates,
                "folder" => settings.SyncFolders,
                _ => false
            };
        }

        // ==================== Entity → Dictionary Mappers ====================

        private static Dictionary<string, object?> DocumentToDict(Document doc)
        {
            return new Dictionary<string, object?>
            {
                ["local_id"] = doc.Id,
                ["document_number"] = doc.Number,
                ["title"] = doc.Title,
                ["subject"] = doc.Subject,
                ["document_type"] = doc.Type.ToString(),
                ["direction"] = doc.Direction.ToString(),
                ["category"] = doc.Category,
                ["urgency_level"] = doc.UrgencyLevel.ToString(),
                ["security_level"] = doc.SecurityLevel.ToString(),
                // VB đến
                ["arrival_number"] = doc.ArrivalNumber,
                ["arrival_date"] = doc.ArrivalDate?.ToUniversalTime().ToString("o"),
                // Xử lý
                ["assigned_to"] = doc.AssignedTo,
                ["processing_notes"] = doc.ProcessingNotes,
                ["due_date"] = doc.DueDate?.ToUniversalTime().ToString("o"),
                // Căn cứ pháp lý
                ["based_on"] = doc.BasedOn?.ToArray(),
                // Nội dung
                ["content"] = doc.Content,
                // Tổ chức
                ["issuer"] = doc.Issuer,
                ["signed_by"] = doc.SignedBy,
                ["signing_title"] = doc.SigningTitle,
                ["signing_authority"] = doc.SigningAuthority,
                ["location"] = doc.Location,
                ["department_id"] = doc.DepartmentId,
                ["department_name"] = doc.DepartmentName,
                ["is_public"] = doc.IsPublic,
                // Workflow
                ["status"] = doc.Status,
                ["workflow_status"] = doc.WorkflowStatus.ToString(),
                ["issue_date"] = doc.IssueDate.ToUniversalTime().ToString("o"),
                ["approved_by"] = doc.ApprovedBy,
                ["approved_date"] = doc.ApprovedDate?.ToUniversalTime().ToString("o"),
                ["signed_date"] = doc.SignedDate?.ToUniversalTime().ToString("o"),
                ["published_by"] = doc.PublishedBy,
                ["published_date"] = doc.PublishedDate?.ToUniversalTime().ToString("o"),
                ["workflow_comments"] = doc.WorkflowComments,
                // Cá nhân
                ["my_status"] = doc.MyStatus.ToString(),
                ["is_starred"] = doc.IsStarred,
                ["personal_priority"] = doc.PersonalPriority,
                ["personal_deadline"] = doc.PersonalDeadline?.ToUniversalTime().ToString("o"),
                ["personal_note"] = doc.PersonalNote,
                ["personal_notes"] = doc.Notes?.Count > 0 ? JsonSerializer.Serialize(doc.Notes) : null,
                // Tags & metadata
                ["tags"] = doc.Tags?.ToArray(),
                ["recipients"] = doc.Recipients?.ToArray(),
                ["related_document_ids"] = doc.RelatedDocumentIds?.ToArray(),
                ["attachment_ids"] = doc.AttachmentIds?.ToArray(),
                ["folder_id"] = doc.FolderId,
                // Bản sao — Theo Điều 25-27, NĐ 30/2020
                ["copy_type"] = doc.CopyType.ToString(),
                ["original_document_id"] = doc.OriginalDocumentId,
                ["copy_number"] = doc.CopyNumber,
                ["copy_symbol"] = doc.CopySymbol,
                ["copy_date"] = doc.CopyDate?.ToUniversalTime().ToString("o"),
                ["copied_by"] = doc.CopiedBy,
                ["copy_signing_title"] = doc.CopySigningTitle,
                ["copy_notes"] = doc.CopyNotes,
                // AI
                ["ai_summary"] = doc.Summary,
                // Soft delete
                ["is_deleted"] = doc.IsDeleted,
                ["local_updated_at"] = (doc.ModifiedDate ?? doc.CreatedDate).ToUniversalTime().ToString("o"),
            };
        }

        private static Dictionary<string, object?> MeetingToDict(Meeting m)
        {
            return new Dictionary<string, object?>
            {
                ["local_id"] = m.Id,
                ["title"] = m.Title,
                ["meeting_number"] = m.MeetingNumber,
                ["meeting_type"] = m.Type.ToString(),
                ["meeting_level"] = m.Level.ToString(),
                ["status"] = m.Status.ToString(),
                ["priority"] = m.Priority,
                // Thời gian
                ["start_time"] = m.StartTime.ToUniversalTime().ToString("o"),
                ["end_time"] = m.EndTime?.ToUniversalTime().ToString("o"),
                ["is_all_day"] = m.IsAllDay,
                // Địa điểm
                ["location"] = m.Location,
                ["meeting_format"] = m.Format.ToString(),
                ["online_link"] = m.OnlineLink,
                // Người tham dự
                ["chair_person"] = m.ChairPerson,
                ["chair_person_title"] = m.ChairPersonTitle,
                ["secretary"] = m.Secretary,
                ["organizing_unit"] = m.OrganizingUnit,
                // JSONB nested data
                ["attendees"] = m.Attendees?.Count > 0 ? JsonSerializer.Serialize(m.Attendees) : null,
                ["tasks"] = m.Tasks?.Count > 0 ? JsonSerializer.Serialize(m.Tasks) : null,
                ["documents"] = m.Documents?.Count > 0 ? JsonSerializer.Serialize(m.Documents) : null,
                // Nội dung
                ["agenda"] = m.Agenda,
                ["content"] = m.Content,
                ["conclusion"] = m.Conclusion,
                ["personal_notes"] = m.PersonalNotes,
                // Tags & liên kết
                ["tags"] = m.Tags?.ToArray(),
                ["related_album_ids"] = m.RelatedAlbumIds?.ToArray(),
                ["attachment_paths"] = m.AttachmentPaths?.ToArray(),
                // Legacy (backward compat)
                ["invitation_doc_id"] = m.InvitationDocId,
                ["minutes_doc_id"] = m.MinutesDocId,
                ["conclusion_doc_id"] = m.ConclusionDocId,
                ["related_document_ids"] = m.RelatedDocumentIds?.ToArray(),
                // Template
                ["is_template"] = m.IsTemplate,
                ["template_name"] = m.TemplateName,
                // Reminder
                ["reminder_minutes_before"] = m.ReminderMinutesBefore,
                ["local_updated_at"] = (m.ModifiedDate ?? m.CreatedDate).ToUniversalTime().ToString("o"),
            };
        }

        private static Dictionary<string, object?> TemplateToDict(DocumentTemplate t)
        {
            return new Dictionary<string, object?>
            {
                ["local_id"] = t.Id,
                ["name"] = t.Name,
                ["description"] = t.Description,
                ["document_type"] = t.Type.ToString(),
                ["category"] = t.Category,
                ["content"] = t.Content,
                ["template_content"] = t.TemplateContent,
                ["ai_prompt"] = t.AIPrompt,
                ["required_fields"] = t.RequiredFields?.ToArray(),
                ["tags"] = t.Tags?.ToArray(),
                ["usage_count"] = t.UsageCount,
                ["store_id"] = t.StoreId,
                ["store_version"] = t.StoreVersion,
                ["local_updated_at"] = (t.ModifiedDate ?? t.CreatedDate).ToUniversalTime().ToString("o"),
            };
        }

        private static Dictionary<string, object?> FolderToDict(Folder f)
        {
            return new Dictionary<string, object?>
            {
                ["local_id"] = f.Id,
                ["name"] = f.Name,
                ["parent_id"] = f.ParentId,
                ["path"] = f.Path,
                ["sort_order"] = f.SortOrder,
                ["icon"] = f.Icon,
                ["color"] = f.Color,
                ["organization_name"] = f.OrganizationName,
                ["local_updated_at"] = f.CreatedDate.ToUniversalTime().ToString("o"),
            };
        }

        // ==================== Settings ====================

        public static CloudSyncSettings GetSyncSettings()
        {
            return AppSettingsService.Load().CloudSync;
        }

        public static void SaveSyncSettings(CloudSyncSettings settings)
        {
            var appSettings = AppSettingsService.Load();
            appSettings.CloudSync = settings;
            AppSettingsService.Save(appSettings);
        }

        // ==================== Thống kê ====================

        /// <summary>
        /// Lấy thống kê sync status trên local.
        /// </summary>
        public Dictionary<string, int> GetSyncStatistics()
        {
            var all = _syncCol.FindAll().ToList();
            return new Dictionary<string, int>
            {
                ["total"] = all.Count,
                ["synced"] = all.Count(m => m.State == SyncState.Synced),
                ["pending"] = all.Count(m => m.State == SyncState.LocalOnly || m.State == SyncState.LocalModified),
                ["conflicted"] = all.Count(m => m.State == SyncState.Conflicted),
                ["failed"] = all.Count(m => m.State == SyncState.Failed),
                ["deleted"] = all.Count(m => m.State == SyncState.LocalDeleted),
            };
        }

        public void Dispose()
        {
            StopAutoSync();
            _api.Dispose();
        }
    }
}
