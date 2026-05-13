using AIVanBan.Core.Data;
using AIVanBan.Core.Models;
using LiteDB;

namespace AIVanBan.Core.Services;

/// <summary>
/// Static helper — đánh dấu thay đổi local để Cloud Sync Engine biết push lên cloud.
/// Gọi sau mỗi CRUD operation trong DocumentService, MeetingService, v.v.
///
/// Thread-safe, dùng chung LiteDB instance từ DatabaseFactory.
/// Nếu Cloud Sync chưa bật → mọi lệnh đều no-op (không gây lỗi).
/// </summary>
public static class SyncTracker
{
    private static readonly object _lock = new();

    /// <summary>
    /// Entity types hợp lệ (phải khớp với CloudSyncService.TABLE_MAP trên API)
    /// </summary>
    public static class EntityTypes
    {
        public const string Document = "document";
        public const string Meeting = "meeting";
        public const string Template = "template";
        public const string Folder = "folder";
        public const string Photo = "photo";
        public const string Album = "album";
    }

    /// <summary>
    /// Đánh dấu entity đã thay đổi (insert hoặc update) trên local.
    /// Gọi sau khi Add/Update thành công.
    /// </summary>
    public static void MarkChanged(string localId, string entityType)
    {
        if (string.IsNullOrEmpty(localId)) return;
        if (!IsSyncEnabled()) return;

        try
        {
            lock (_lock)
            {
                var col = GetSyncCollection();
                var meta = col.FindOne(x => x.LocalId == localId && x.EntityType == entityType);

                if (meta == null)
                {
                    meta = new SyncMetadata
                    {
                        LocalId = localId,
                        EntityType = entityType,
                        State = SyncState.LocalOnly,
                        LocalUpdatedAt = DateTime.UtcNow,
                    };
                    col.Insert(meta);
                }
                else
                {
                    if (meta.State == SyncState.Synced)
                        meta.State = SyncState.LocalModified;
                    meta.LocalUpdatedAt = DateTime.UtcNow;
                    col.Update(meta);
                }
            }
        }
        catch
        {
            // Sync tracking không được phá luồng chính — nuốt exception
        }
    }

    /// <summary>
    /// Đánh dấu entity đã xóa trên local.
    /// Gọi sau khi Delete thành công.
    /// </summary>
    public static void MarkDeleted(string localId, string entityType)
    {
        if (string.IsNullOrEmpty(localId)) return;
        if (!IsSyncEnabled()) return;

        try
        {
            lock (_lock)
            {
                var col = GetSyncCollection();
                var meta = col.FindOne(x => x.LocalId == localId && x.EntityType == entityType);

                if (meta != null)
                {
                    meta.State = SyncState.LocalDeleted;
                    meta.LocalUpdatedAt = DateTime.UtcNow;
                    col.Update(meta);
                }
                // Nếu chưa có record sync (chưa bao giờ sync) thì không cần đánh dấu xóa
            }
        }
        catch
        {
            // Sync tracking không được phá luồng chính
        }
    }

    /// <summary>
    /// Kiểm tra sync đã bật chưa — cache kết quả 30 giây.
    /// </summary>
    private static bool _cachedEnabled;
    private static DateTime _cacheExpiry = DateTime.MinValue;

    private static bool IsSyncEnabled()
    {
        if (DateTime.UtcNow < _cacheExpiry) return _cachedEnabled;

        try
        {
            var settings = AppSettingsService.Load();
            _cachedEnabled = settings.CloudSync?.Enabled == true;
        }
        catch
        {
            _cachedEnabled = false;
        }

        _cacheExpiry = DateTime.UtcNow.AddSeconds(30);
        return _cachedEnabled;
    }

    private static ILiteCollection<SyncMetadata> GetSyncCollection()
    {
        var db = DatabaseFactory.GetDatabase();
        var col = db.GetCollection<SyncMetadata>("sync_metadata");
        col.EnsureIndex(x => x.LocalId);
        col.EnsureIndex(x => x.EntityType);
        return col;
    }

    /// <summary>
    /// Đặt lại cache — gọi khi user thay đổi setting Cloud Sync
    /// </summary>
    public static void InvalidateCache()
    {
        _cacheExpiry = DateTime.MinValue;
    }
}
