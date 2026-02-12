namespace AIVanBan.Core.Models;

/// <summary>
/// Mẫu văn bản
/// </summary>
public class DocumentTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Thông tin cơ bản
    public string Name { get; set; } = string.Empty; // "Mẫu công văn điều động cán bộ"
    public DocumentType Type { get; set; }
    public string Category { get; set; } = string.Empty; // "Nội vụ"
    
    // Nội dung mẫu
    public string Content { get; set; } = string.Empty; // Nội dung Word/Plain text
    public string TemplateContent { get; set; } = string.Empty; // Nội dung mẫu hiển thị
    public string FilePath { get; set; } = string.Empty; // File .docx mẫu
    
    // Prompt cho AI
    public string AIPrompt { get; set; } = string.Empty; // Prompt template cho Gemini
    public string[] RequiredFields { get; set; } = Array.Empty<string>(); // ["nguoi_gui", "nguoi_nhan"]
    
    // Metadata
    public string Description { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public int UsageCount { get; set; } // Số lần sử dụng
    
    // Store (kho mẫu online)
    public string StoreId { get; set; } = string.Empty; // ID trên store, rỗng = tự tạo
    public int StoreVersion { get; set; } // Version trên store, 0 = chưa từ store
    
    // Audit
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? ModifiedDate { get; set; }
}
