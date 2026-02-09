namespace AIVanBan.Core.Models;

/// <summary>
/// Album ảnh đơn giản - tự do tạo, không theo template
/// </summary>
public class SimpleAlbum
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Tiêu đề album (ví dụ: "Hội nghị cán bộ 2026")
    /// </summary>
    public string Title { get; set; } = "";
    
    /// <summary>
    /// Mô tả album (có thể rất dài như bài báo)
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// Đường dẫn thư mục chứa ảnh
    /// </summary>
    public string FolderPath { get; set; } = "";
    
    /// <summary>
    /// ID của AlbumFolder chứa album này
    /// Rỗng nếu album không thuộc folder nào (legacy albums)
    /// </summary>
    public string AlbumFolderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Đường dẫn đầy đủ trong cây folder
    /// Vd: "Trường Tiểu học/ALBUM ẢNH/Hoạt động giảng dạy"
    /// </summary>
    public string AlbumFolderPath { get; set; } = string.Empty;
    
    /// <summary>
    /// Đường dẫn ảnh bìa (cover photo)
    /// </summary>
    public string? CoverPhotoPath { get; set; }
    
    /// <summary>
    /// Tags của album
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Ngày tạo
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Ngày cập nhật
    /// </summary>
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Số lượng ảnh trong album
    /// </summary>
    public int PhotoCount { get; set; }
    
    /// <summary>
    /// Trạng thái (Active, Archived, Deleted)
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Danh sách thumbnail photos (tối đa 4 ảnh để hiển thị preview)
    /// </summary>
    public List<string> ThumbnailPhotos { get; set; } = new();
}
