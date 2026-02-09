namespace AIVanBan.Core.Models;

/// <summary>
/// Folder quản lý Album ảnh theo cấu trúc cây nhiều cấp
/// Tương tự Folder của Document nhưng dành riêng cho Album
/// </summary>
public class AlbumFolder
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Tên folder (vd: "Trường Tiểu học", "ALBUM ẢNH", "Hoạt động giảng dạy")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// ID folder cha (rỗng = root folder)
    /// </summary>
    public string ParentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Đường dẫn đầy đủ (vd: "Trường Tiểu học/ALBUM ẢNH/Hoạt động giảng dạy")
    /// Tự động build khi tạo folder
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Icon hiển thị
    /// </summary>
    public string Icon { get; set; } = "📁";
    
    /// <summary>
    /// Màu sắc folder
    /// </summary>
    public string Color { get; set; } = "#FF9800"; // Orange cho album
    
    /// <summary>
    /// Thứ tự sắp xếp
    /// </summary>
    public int SortOrder { get; set; } = 0;
    
    /// <summary>
    /// Số lượng album trong folder này (không đệ quy)
    /// </summary>
    public int AlbumCount { get; set; }
    
    /// <summary>
    /// Tổng số ảnh trong tất cả album (bao gồm subfolder)
    /// </summary>
    public int TotalPhotoCount { get; set; }
    
    /// <summary>
    /// Đường dẫn ảnh cover của folder (lấy từ album đầu tiên)
    /// </summary>
    public string? CoverPhotoPath { get; set; }
    
    /// <summary>
    /// Loại folder từ template
    /// "Organization" | "Category" | "SubCategory" | "Custom"
    /// </summary>
    public string FolderType { get; set; } = "Custom";
    
    /// <summary>
    /// Link với AlbumStructureTemplate (nếu tạo từ template)
    /// </summary>
    public string? TemplateId { get; set; }
    public string? CategoryId { get; set; }
    public string? SubCategoryId { get; set; }
    
    /// <summary>
    /// Mô tả folder
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Audit
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public string ModifiedBy { get; set; } = Environment.UserName;
}
