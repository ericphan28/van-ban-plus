using System;
using System.Collections.Generic;

namespace AIVanBan.Core.Models
{
    // ==================== Sync Models ====================

    /// <summary>
    /// Trạng thái sync của một entity giữa local ↔ cloud.
    /// </summary>
    public enum SyncState
    {
        /// <summary>Chưa đồng bộ — chỉ có trên local</summary>
        LocalOnly,
        /// <summary>Đã đồng bộ — local = cloud</summary>
        Synced,
        /// <summary>Đã sửa trên local — cần push lên cloud</summary>
        LocalModified,
        /// <summary>Đã sửa trên cloud — cần pull về local</summary>
        CloudModified,
        /// <summary>Conflict — cả local và cloud đều thay đổi</summary>
        Conflicted,
        /// <summary>Đã xóa trên local — cần xóa trên cloud</summary>
        LocalDeleted,
        /// <summary>Đang đồng bộ</summary>
        Syncing,
        /// <summary>Sync thất bại</summary>
        Failed
    }

    /// <summary>
    /// Metadata sync cho mỗi entity.
    /// Lưu kèm trong LiteDB collection riêng.
    /// </summary>
    public class SyncMetadata
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>ID của entity trong LiteDB (Document.Id, Meeting.Id, ...)</summary>
        public string LocalId { get; set; } = "";
        
        /// <summary>UUID trên Supabase</summary>
        public string? CloudId { get; set; }
        
        /// <summary>Loại entity</summary>
        public string EntityType { get; set; } = ""; // "document", "meeting", "template", "folder"
        
        /// <summary>Trạng thái sync hiện tại</summary>
        public SyncState State { get; set; } = SyncState.LocalOnly;
        
        /// <summary>Thời điểm sửa cuối cùng trên local</summary>
        public DateTime LocalUpdatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>Thời điểm sửa cuối cùng trên cloud</summary>
        public DateTime? CloudUpdatedAt { get; set; }
        
        /// <summary>Thời điểm sync thành công cuối cùng</summary>
        public DateTime? LastSyncedAt { get; set; }
        
        /// <summary>Version number trên cloud</summary>
        public int SyncVersion { get; set; } = 0;
        
        /// <summary>Số lần sync thất bại liên tiếp</summary>
        public int FailCount { get; set; } = 0;
        
        /// <summary>Lỗi lần sync cuối</summary>
        public string? LastError { get; set; }
    }

    /// <summary>
    /// Kết quả sync.
    /// </summary>
    public class SyncResult
    {
        public string SyncId { get; set; } = "";
        public string Status { get; set; } = ""; // started, completed, failed, partial
        public int ItemsPushed { get; set; }
        public int ItemsPulled { get; set; }
        public int ItemsConflicted { get; set; }
        public List<SyncConflict> Conflicts { get; set; } = new();
        public string SyncTimestamp { get; set; } = "";
        public int DurationMs { get; set; }
    }

    /// <summary>
    /// Conflict khi sync.
    /// </summary>
    public class SyncConflict
    {
        public string EntityType { get; set; } = "";
        public string LocalId { get; set; } = "";
        public Dictionary<string, object?> CloudVersion { get; set; } = new();
        public Dictionary<string, object?> LocalVersion { get; set; } = new();
        public string Resolution { get; set; } = "cloud_wins"; // cloud_wins, local_wins, manual
    }

    /// <summary>
    /// Entity gửi lên cloud khi push.
    /// </summary>
    public class SyncEntity
    {
        public string EntityType { get; set; } = ""; // document, meeting, template, folder
        public string Action { get; set; } = "upsert"; // upsert, delete
        public string LocalId { get; set; } = "";
        public string LocalUpdatedAt { get; set; } = "";
        public Dictionary<string, object?> Data { get; set; } = new();
    }

    /// <summary>
    /// Change từ cloud khi pull.
    /// </summary>
    public class SyncChange
    {
        public string EntityType { get; set; } = "";
        public string EntityId { get; set; } = "";
        public string LocalId { get; set; } = "";
        public string Action { get; set; } = ""; // upsert, delete
        public string UpdatedAt { get; set; } = "";
        public Dictionary<string, object?> Data { get; set; } = new();
    }

    /// <summary>
    /// Response từ pull API.
    /// </summary>
    public class SyncPullResponse
    {
        public List<SyncChange> Changes { get; set; } = new();
        public string SyncTimestamp { get; set; } = "";
        public bool HasMore { get; set; }
    }

    /// <summary>
    /// Thông tin thiết bị.
    /// </summary>
    public class DeviceInfo
    {
        public string DeviceId { get; set; } = "";
        public string DeviceName { get; set; } = "";
        public string DeviceType { get; set; } = "desktop";
        public string OsInfo { get; set; } = "";
        public string AppVersion { get; set; } = "";
    }

    /// <summary>
    /// Thông tin quota storage trên cloud.
    /// </summary>
    public class StorageQuotaInfo
    {
        public long UsedBytes { get; set; }
        public long LimitBytes { get; set; }
        public double UsedPercent { get; set; }
        public string UsedDisplay { get; set; } = "";
        public int DocumentsCount { get; set; }
        public int AttachmentsCount { get; set; }
        public int PhotosCount { get; set; }
        public bool IsExceeded { get; set; }
    }

    /// <summary>
    /// Thông tin cloud backup.
    /// </summary>
    public class CloudBackupInfo
    {
        public string Id { get; set; } = "";
        public string BackupType { get; set; } = ""; // auto, manual, scheduled
        public string Status { get; set; } = ""; // pending, in_progress, completed, failed
        public int DocumentsCount { get; set; }
        public int MeetingsCount { get; set; }
        public int TemplatesCount { get; set; }
        public int PhotosCount { get; set; }
        public long TotalSizeBytes { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    // ==================== Cloud Settings Extension ====================

    /// <summary>
    /// Settings cho Cloud Sync — thêm vào AppSettings.
    /// </summary>
    public class CloudSyncSettings
    {
        /// <summary>Bật/tắt cloud sync</summary>
        public bool Enabled { get; set; } = false;
        
        /// <summary>Sync tự động (background)</summary>
        public bool AutoSyncEnabled { get; set; } = true;
        
        /// <summary>Khoảng cách sync tự động (phút)</summary>
        public int AutoSyncIntervalMinutes { get; set; } = 5;
        
        /// <summary>Sync khi mở app</summary>
        public bool SyncOnStartup { get; set; } = true;
        
        /// <summary>Sync khi đóng app</summary>
        public bool SyncOnExit { get; set; } = true;
        
        /// <summary>Device ID duy nhất cho máy này</summary>
        public string DeviceId { get; set; } = "";
        
        /// <summary>Tên thiết bị (hiển thị)</summary>
        public string DeviceName { get; set; } = "";
        
        /// <summary>Timestamp lần sync cuối thành công</summary>
        public DateTime? LastSyncTimestamp { get; set; }
        
        /// <summary>Sync documents</summary>
        public bool SyncDocuments { get; set; } = true;
        
        /// <summary>Sync meetings</summary>
        public bool SyncMeetings { get; set; } = true;
        
        /// <summary>Sync templates</summary>
        public bool SyncTemplates { get; set; } = true;
        
        /// <summary>Sync folders</summary>
        public bool SyncFolders { get; set; } = true;
        
        /// <summary>Sync attachments (files)</summary>
        public bool SyncAttachments { get; set; } = false;
        
        /// <summary>Sync photos</summary>
        public bool SyncPhotos { get; set; } = false;
        
        /// <summary>Giải quyết conflict: auto (cloud wins) / manual</summary>
        public string ConflictResolution { get; set; } = "auto";
    }
}
