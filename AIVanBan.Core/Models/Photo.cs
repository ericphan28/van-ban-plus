namespace AIVanBan.Core.Models;

/// <summary>
/// Ảnh trong album
/// </summary>
public class Photo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Thông tin file
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = "image/jpeg";
    
    // Metadata
    public DateTime DateTaken { get; set; } = DateTime.Now;
    public string Event { get; set; } = string.Empty; // "Lễ khánh thành"
    public string Location { get; set; } = string.Empty; // "Trường TH Hòa Bình"
    public string Photographer { get; set; } = Environment.UserName;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string Description { get; set; } = string.Empty;
    public string[] People { get; set; } = Array.Empty<string>(); // Người trong ảnh
    
    // Liên kết
    public string RelatedDocumentId { get; set; } = string.Empty;
    public string AlbumId { get; set; } = string.Empty;
    
    // Thumbnail
    public byte[] ThumbnailData { get; set; } = Array.Empty<byte>();
    
    // Audit
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}

/// <summary>
/// Album ảnh
/// </summary>
public class Album
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParentId { get; set; } = string.Empty; // Hierarchical structure
    public string Path { get; set; } = string.Empty; // Full path for display
    public string Icon { get; set; } = "📁"; // Icon for folder
    public DateTime EventDate { get; set; } = DateTime.Now;
    public string CoverPhotoId { get; set; } = string.Empty; // Ảnh bìa
    public int PhotoCount { get; set; }
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
