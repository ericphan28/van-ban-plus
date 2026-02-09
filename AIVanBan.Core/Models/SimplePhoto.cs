using System.Windows.Media.Imaging;

namespace AIVanBan.Core.Models;

/// <summary>
/// Thông tin một ảnh trong album
/// </summary>
public class SimplePhoto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Album chứa ảnh này
    /// </summary>
    public string AlbumId { get; set; } = "";
    
    /// <summary>
    /// Đường dẫn file ảnh gốc
    /// </summary>
    public string FilePath { get; set; } = "";
    
    /// <summary>
    /// BitmapImage không lock file - dùng cho hiển thị
    /// </summary>
    public BitmapImage? ImageSource { get; set; }
    
    /// <summary>
    /// Tên file
    /// </summary>
    public string FileName { get; set; } = "";
    
    /// <summary>
    /// Đường dẫn thumbnail (cache)
    /// </summary>
    public string? ThumbnailPath { get; set; }
    
    /// <summary>
    /// Tiêu đề ảnh
    /// </summary>
    public string Title { get; set; } = "";
    
    /// <summary>
    /// Mô tả ảnh
    /// </summary>
    public string Description { get; set; } = "";
    
    /// <summary>
    /// Tags của ảnh
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Ngày chụp (từ EXIF)
    /// </summary>
    public DateTime? DateTaken { get; set; }
    
    /// <summary>
    /// Kích thước ảnh
    /// </summary>
    public int Width { get; set; }
    public int Height { get; set; }
    
    /// <summary>
    /// Kích thước file (bytes)
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Camera/thiết bị chụp
    /// </summary>
    public string? CameraModel { get; set; }
    
    /// <summary>
    /// Ngày thêm vào album
    /// </summary>
    public DateTime AddedDate { get; set; } = DateTime.Now;
}
