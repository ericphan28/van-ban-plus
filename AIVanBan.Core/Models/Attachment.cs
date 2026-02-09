namespace AIVanBan.Core.Models;

/// <summary>
/// File đính kèm của văn bản (Word, PDF, Excel, v.v.)
/// </summary>
public class Attachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Thông tin file
    public string FileName { get; set; } = string.Empty; // Tên file gốc
    public string FilePath { get; set; } = string.Empty; // Đường dẫn lưu trữ
    public string FileExtension { get; set; } = string.Empty; // .docx, .pdf, .xlsx
    public long FileSize { get; set; } // Size tính bằng bytes
    
    // Phân loại
    public AttachmentType Type { get; set; } = AttachmentType.Word;
    public string Description { get; set; } = string.Empty; // Mô tả file
    public bool IsPrimary { get; set; } = false; // File chính (văn bản gốc)
    
    // Metadata
    public string DocumentId { get; set; } = string.Empty; // ID văn bản chủ
    public int PageCount { get; set; } // Số trang (nếu có)
    public string MimeType { get; set; } = string.Empty; // application/pdf, application/vnd.openxmlformats-officedocument.wordprocessingml.document
    
    // Audit
    public string UploadedBy { get; set; } = Environment.UserName;
    public DateTime UploadedDate { get; set; } = DateTime.Now;
    
    // Security
    public bool IsEncrypted { get; set; } = false;
    public string ChecksumMD5 { get; set; } = string.Empty; // Để verify file integrity
}

/// <summary>
/// Loại file đính kèm
/// </summary>
public enum AttachmentType
{
    Word,       // .docx, .doc - Văn bản Word
    PDF,        // .pdf - File PDF  
    Excel,      // .xlsx, .xls - Bảng tính Excel
    PowerPoint, // .pptx, .ppt - Bản trình bày PowerPoint
    Image,      // .jpg, .png, .gif, .bmp - Ảnh scan văn bản
    SignedPDF,  // .pdf - File PDF đã ký số
    Other       // Loại khác
}
