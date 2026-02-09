namespace AIVanBan.Core.Models;

/// <summary>
/// Tag để phân loại album/ảnh
/// </summary>
public class PhotoTag
{
    /// <summary>
    /// Tên tag (ví dụ: "Hội nghị", "Kiểm tra", "Tuyên truyền")
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Màu hiển thị (hex color)
    /// </summary>
    public string Color { get; set; } = "#2196F3";
    
    /// <summary>
    /// Số lần sử dụng
    /// </summary>
    public int UsageCount { get; set; }
    
    /// <summary>
    /// Ngày tạo
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
