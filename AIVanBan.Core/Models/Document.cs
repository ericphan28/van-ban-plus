namespace AIVanBan.Core.Models;

/// <summary>
/// Văn bản/Tài liệu
/// </summary>
public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Thông tin cơ bản
    public string Title { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty; // Số văn bản: 123/CV-UBND
    public DateTime IssueDate { get; set; } = DateTime.Now;
    public string Issuer { get; set; } = string.Empty; // Cơ quan ban hành
    public string Subject { get; set; } = string.Empty; // Trích yếu
    public string[] Recipients { get; set; } = Array.Empty<string>(); // Nơi nhận/Nơi gửi (Đồng kính gởi)
    
    // Phân loại
    public DocumentType Type { get; set; }
    public string Category { get; set; } = string.Empty; // Lĩnh vực
    public Direction Direction { get; set; } // Đi/Đến
    
    // CĂN CỨ - Phần quan trọng trong văn bản hành chính VN
    public string[] BasedOn { get; set; } = Array.Empty<string>(); // Các căn cứ pháp lý (mỗi căn cứ một dòng)
    
    // Nội dung
    public string Content { get; set; } = string.Empty; // Full text để search
    public string FilePath { get; set; } = string.Empty; // Đường dẫn file gốc (deprecated - dùng Attachments)
    public string FileExtension { get; set; } = string.Empty; // .docx, .pdf (deprecated)
    public long FileSize { get; set; } // (deprecated)
    
    // File đính kèm (NEW - support multiple files)
    public string[] AttachmentIds { get; set; } = Array.Empty<string>(); // IDs of Attachment objects
    
    // Metadata
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string[] RelatedDocumentIds { get; set; } = Array.Empty<string>(); // Văn bản liên quan
    public string Status { get; set; } = "Còn hiệu lực"; // Còn/Hết hiệu lực
    public string FolderId { get; set; } = string.Empty; // Thư mục chứa văn bản
    
    // Phòng ban & Phân quyền
    public string DepartmentId { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = false; // Văn bản công khai - ai cũng xem được
    
    // Workflow - Quy trình phê duyệt văn bản đi
    public DocumentStatus WorkflowStatus { get; set; } = DocumentStatus.Draft;
    public string ApprovedBy { get; set; } = string.Empty; // User ID người duyệt
    public DateTime? ApprovedDate { get; set; }
    public string SignedBy { get; set; } = string.Empty; // Họ tên người ký
    public string SigningTitle { get; set; } = string.Empty; // Chức danh ký (VD: CHỦ TỊCH, GIÁM ĐỐC, TRƯỞNG PHÒNG)
    public string SigningAuthority { get; set; } = string.Empty; // Thẩm quyền ký (VD: TM., KT., Q., hoặc rỗng nếu ký trực tiếp)
    public string Location { get; set; } = string.Empty; // Địa danh ban hành (VD: Gia Kiểm, Hà Nội, TP. Hồ Chí Minh)
    public DateTime? SignedDate { get; set; }
    public string PublishedBy { get; set; } = string.Empty; // User ID người phát hành
    public DateTime? PublishedDate { get; set; }
    public string WorkflowComments { get; set; } = string.Empty; // JSON array of comments
    
    // Audit
    public string CreatedBy { get; set; } = Environment.UserName;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string ModifiedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    
    // Search & AI
    public float[] Embedding { get; set; } = Array.Empty<float>(); // Vector để semantic search
    public string Summary { get; set; } = string.Empty; // Tóm tắt AI
}

/// <summary>
/// Loại văn bản
/// </summary>
public enum DocumentType
{
    Luat,           // Luật
    NghiDinh,       // Nghị định
    ThongTu,        // Thông tư
    NghiQuyet,      // Nghị quyết
    QuyetDinh,      // Quyết định
    CongVan,        // Công văn
    BaoCao,         // Báo cáo
    ToTrinh,        // Tờ trình
    KeHoach,        // Kế hoạch
    ThongBao,       // Thông báo
    ChiThi,         // Chỉ thị
    HuongDan,       // Hướng dẫn
    QuyDinh,        // Quy định
    Khac            // Khác
}

/// <summary>
/// Hướng văn bản
/// </summary>
public enum Direction
{
    Di,     // Văn bản đi
    Den,    // Văn bản đến
    NoiBo   // Nội bộ
}

/// <summary>
/// Helper class để hiển thị tên tiếng Việt thân thiện cho enum
/// </summary>
public static class EnumDisplayHelper
{
    private static readonly Dictionary<DocumentType, string> _typeNames = new()
    {
        [DocumentType.Luat] = "Luật",
        [DocumentType.NghiDinh] = "Nghị định",
        [DocumentType.ThongTu] = "Thông tư",
        [DocumentType.NghiQuyet] = "Nghị quyết",
        [DocumentType.QuyetDinh] = "Quyết định",
        [DocumentType.CongVan] = "Công văn",
        [DocumentType.BaoCao] = "Báo cáo",
        [DocumentType.ToTrinh] = "Tờ trình",
        [DocumentType.KeHoach] = "Kế hoạch",
        [DocumentType.ThongBao] = "Thông báo",
        [DocumentType.ChiThi] = "Chỉ thị",
        [DocumentType.HuongDan] = "Hướng dẫn",
        [DocumentType.QuyDinh] = "Quy định",
        [DocumentType.Khac] = "Khác",
    };

    private static readonly Dictionary<Direction, string> _dirNames = new()
    {
        [Direction.Di] = "📤 Văn bản đi",
        [Direction.Den] = "📥 Văn bản đến",
        [Direction.NoiBo] = "🔄 Nội bộ",
    };

    public static string GetDisplayName(this DocumentType type) =>
        _typeNames.TryGetValue(type, out var name) ? name : type.ToString();

    public static string GetDisplayName(this Direction dir) =>
        _dirNames.TryGetValue(dir, out var name) ? name : dir.ToString();

    /// <summary>
    /// Tạo danh sách {Value, Display} cho ComboBox DocumentType
    /// </summary>
    public static List<KeyValuePair<DocumentType, string>> GetDocumentTypeItems() =>
        _typeNames.Select(kv => new KeyValuePair<DocumentType, string>(kv.Key, kv.Value)).ToList();

    /// <summary>
    /// Tạo danh sách {Value, Display} cho ComboBox Direction
    /// </summary>
    public static List<KeyValuePair<Direction, string>> GetDirectionItems() =>
        _dirNames.Select(kv => new KeyValuePair<Direction, string>(kv.Key, kv.Value)).ToList();
}

/// <summary>
/// Trạng thái workflow văn bản đi
/// </summary>
public enum DocumentStatus
{
    Draft,              // Nháp - đang soạn
    PendingApproval,    // Trình ký - chờ duyệt
    Approved,           // Đã duyệt - chờ ký
    Signed,             // Đã ký - chờ phát hành
    Published,          // Đã phát hành - có số VB
    Sent,               // Đã gửi đi
    Archived            // Đã lưu trữ
}
