namespace AIVanBan.Core.Models;

/// <summary>
/// Cấu trúc album theo nghiệp vụ cơ quan
/// Có thể đồng bộ từ server hoặc tạo local
/// </summary>
public class AlbumStructureTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty; // "Cấu trúc cơ quan Xã/Phường"
    public string OrganizationType { get; set; } = string.Empty; // "XaPhuong", "Huyen", "Tinh", "HoiNongDan", v.v.
    public string Version { get; set; } = "1.0"; // Version để update
    public string Description { get; set; } = string.Empty;
    
    // Danh sách các category chính
    public List<AlbumCategory> Categories { get; set; } = new();
    
    // Metadata
    public string Source { get; set; } = "local"; // "local" hoặc "web-sync"
    public string SyncUrl { get; set; } = string.Empty; // URL để đồng bộ
    public DateTime LastSyncDate { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = Environment.UserName;
    public bool IsActive { get; set; } = true; // Template đang dùng
}

/// <summary>
/// Category của album (cấp 1)
/// </summary>
public class AlbumCategory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty; // "Sự kiện - Hội nghị"
    public string Icon { get; set; } = "📂";
    public int SortOrder { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Subcategories (cấp 2)
    public List<AlbumSubCategory> SubCategories { get; set; } = new();
}

/// <summary>
/// Subcategory của album (cấp 2)
/// </summary>
public class AlbumSubCategory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty; // "Đại hội Đảng bộ"
    public string Icon { get; set; } = "📁";
    public int SortOrder { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool AutoCreateYearFolder { get; set; } = false; // Tự động tạo folder theo năm
    public string[] SuggestedTags { get; set; } = Array.Empty<string>(); // Tags gợi ý
}

/// <summary>
/// Album instance thực tế được tạo từ template
/// Lưu trong database và file system
/// </summary>
public class AlbumInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty; // "[2024] Lễ khánh thành"
    public string FullPath { get; set; } = string.Empty; // "Sự kiện/[2024] Lễ khánh thành"
    public string PhysicalPath { get; set; } = string.Empty; // Đường dẫn vật lý
    
    // Link với template
    public string TemplateId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public string SubCategoryId { get; set; } = string.Empty;
    
    // Metadata
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; } = DateTime.Now;
    public string Location { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string Icon { get; set; } = "📁";
    
    // Stats
    public int PhotoCount { get; set; }
    public long TotalSize { get; set; }
    public string CoverPhotoId { get; set; } = string.Empty;
    
    // Related
    public string[] RelatedDocumentIds { get; set; } = Array.Empty<string>();
    public string[] RelatedProjectIds { get; set; } = Array.Empty<string>();
    
    // Audit
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
}

/// <summary>
/// Photo với metadata đầy đủ
/// </summary>
public class PhotoExtended
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // File info
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty; // Cache thumbnail
    public long FileSize { get; set; }
    public string MimeType { get; set; } = "image/jpeg";
    public int Width { get; set; }
    public int Height { get; set; }
    
    // Album
    public string AlbumId { get; set; } = string.Empty;
    public string AlbumPath { get; set; } = string.Empty;
    
    // Metadata
    public DateTime DateTaken { get; set; } = DateTime.Now;
    public string Event { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public GeoLocation? GeoLocation { get; set; }
    public string Photographer { get; set; } = Environment.UserName;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string Description { get; set; } = string.Empty;
    public string[] People { get; set; } = Array.Empty<string>();
    
    // Categories (từ template)
    public string CategoryId { get; set; } = string.Empty;
    public string SubCategoryId { get; set; } = string.Empty;
    
    // Related entities
    public string[] RelatedDocumentIds { get; set; } = Array.Empty<string>();
    public string[] RelatedProjectIds { get; set; } = Array.Empty<string>();
    
    // Stats
    public int ViewCount { get; set; }
    public bool IsCoverPhoto { get; set; }
    public bool IsFeatured { get; set; }
    public int Rating { get; set; } // 1-5 stars
    
    // Audit
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
}

/// <summary>
/// Tọa độ địa lý
/// </summary>
public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
}
