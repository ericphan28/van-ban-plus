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
    
    // Mức độ khẩn, độ mật — Theo Điều 8 khoản 3b, Phụ lục VI NĐ 30/2020
    public UrgencyLevel UrgencyLevel { get; set; } = UrgencyLevel.Thuong; // Thường/Khẩn/Thượng khẩn/Hỏa tốc
    public SecurityLevel SecurityLevel { get; set; } = SecurityLevel.Thuong; // Thường/Mật/Tối mật/Tuyệt mật
    
    // Quản lý VB đến — Theo Điều 22, Phụ lục VI NĐ 30/2020
    public int ArrivalNumber { get; set; } // Số đến (liên tiếp trong năm)
    public DateTime? ArrivalDate { get; set; } // Ngày đến
    
    // Theo dõi xử lý — Theo Điều 24, Phụ lục VI NĐ 30/2020
    public DateTime? DueDate { get; set; } // Hạn giải quyết
    public string AssignedTo { get; set; } = string.Empty; // Người/đơn vị xử lý chính
    public string ProcessingNotes { get; set; } = string.Empty; // Ý kiến chỉ đạo, trạng thái xử lý
    
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
    
    // Soft delete — Thùng rác
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
    
    // Bản sao — Theo Điều 25-27, NĐ 30/2020/NĐ-CP
    public CopyType CopyType { get; set; } = CopyType.None; // Loại bản sao (None = VB gốc)
    public string OriginalDocumentId { get; set; } = string.Empty; // ID VB gốc (nếu là bản sao)
    public int CopyNumber { get; set; } // Số bản sao (liên tiếp từ 01, chung cho SY/SL/TrS)
    public string CopySymbol { get; set; } = string.Empty; // Ký hiệu bản sao: 05/SY-UBND
    public DateTime? CopyDate { get; set; } // Ngày sao
    public string CopiedBy { get; set; } = string.Empty; // Người ký bản sao
    public string CopySigningTitle { get; set; } = string.Empty; // Chức vụ người ký bản sao
    public string CopyNotes { get; set; } = string.Empty; // Ghi chú (trích sao: phần nội dung trích)
}

/// <summary>
/// Loại văn bản
/// </summary>
/// <summary>
/// Loại văn bản — Theo Điều 7, NĐ 30/2020/NĐ-CP (29 loại VB hành chính)
/// Ký hiệu viết tắt theo Phụ lục III
/// </summary>
public enum DocumentType
{
    // === VĂN BẢN QUY PHẠM PHÁP LUẬT (không thuộc 29 loại VB hành chính, giữ để lưu trữ VB đến) ===
    Luat,           // Luật
    NghiDinh,       // Nghị định
    ThongTu,        // Thông tư
    
    // === 29 LOẠI VĂN BẢN HÀNH CHÍNH — Điều 7, NĐ 30/2020 ===
    NghiQuyet,      // Nghị quyết (cá biệt)       — Ký hiệu: NQ
    QuyetDinh,      // Quyết định (cá biệt)       — Ký hiệu: QĐ
    ChiThi,         // Chỉ thị                     — Ký hiệu: CT
    QuyChE,         // Quy chế                     — Ký hiệu: QC
    QuyDinh,        // Quy định                    — Ký hiệu: QyĐ
    ThongCao,       // Thông cáo                   — Ký hiệu: TC
    ThongBao,       // Thông báo                   — Ký hiệu: TB
    HuongDan,       // Hướng dẫn                   — Ký hiệu: HD
    ChuongTrinh,    // Chương trình                — Ký hiệu: CTr
    KeHoach,        // Kế hoạch                    — Ký hiệu: KH
    PhuongAn,       // Phương án                   — Ký hiệu: PA
    DeAn,           // Đề án                       — Ký hiệu: ĐA
    DuAn,           // Dự án                       — Ký hiệu: DA
    BaoCao,         // Báo cáo                     — Ký hiệu: BC
    BienBan,        // Biên bản                    — Ký hiệu: BB
    ToTrinh,        // Tờ trình                    — Ký hiệu: TTr
    HopDong,        // Hợp đồng                    — Ký hiệu: HĐ
    CongVan,        // Công văn                    — Ký hiệu: CV
    CongDien,       // Công điện                   — Ký hiệu: CĐ
    BanGhiNho,      // Bản ghi nhớ                 — Ký hiệu: BGN
    BanThoaThuan,   // Bản thỏa thuận              — Ký hiệu: BTT
    GiayUyQuyen,    // Giấy ủy quyền               — Ký hiệu: GUQ
    GiayMoi,        // Giấy mời                    — Ký hiệu: GM
    GiayGioiThieu,  // Giấy giới thiệu             — Ký hiệu: GGT
    GiayNghiPhep,   // Giấy nghỉ phép              — Ký hiệu: GNP
    PhieuGui,       // Phiếu gửi                   — Ký hiệu: PG
    PhieuChuyen,    // Phiếu chuyển                — Ký hiệu: PC
    PhieuBao,       // Phiếu báo                   — Ký hiệu: PB
    ThuCong,        // Thư công                    — Ký hiệu: ThC
    
    // === KHÁC ===
    Khac            // Khác (loại VB không thuộc 29 loại trên)
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
        // VBQPPL (giữ để lưu trữ VB đến)
        [DocumentType.Luat] = "Luật",
        [DocumentType.NghiDinh] = "Nghị định",
        [DocumentType.ThongTu] = "Thông tư",
        // 29 loại VB hành chính — Điều 7, NĐ 30/2020/NĐ-CP
        [DocumentType.NghiQuyet] = "Nghị quyết (cá biệt)",
        [DocumentType.QuyetDinh] = "Quyết định (cá biệt)",
        [DocumentType.ChiThi] = "Chỉ thị",
        [DocumentType.QuyChE] = "Quy chế",
        [DocumentType.QuyDinh] = "Quy định",
        [DocumentType.ThongCao] = "Thông cáo",
        [DocumentType.ThongBao] = "Thông báo",
        [DocumentType.HuongDan] = "Hướng dẫn",
        [DocumentType.ChuongTrinh] = "Chương trình",
        [DocumentType.KeHoach] = "Kế hoạch",
        [DocumentType.PhuongAn] = "Phương án",
        [DocumentType.DeAn] = "Đề án",
        [DocumentType.DuAn] = "Dự án",
        [DocumentType.BaoCao] = "Báo cáo",
        [DocumentType.BienBan] = "Biên bản",
        [DocumentType.ToTrinh] = "Tờ trình",
        [DocumentType.HopDong] = "Hợp đồng",
        [DocumentType.CongVan] = "Công văn",
        [DocumentType.CongDien] = "Công điện",
        [DocumentType.BanGhiNho] = "Bản ghi nhớ",
        [DocumentType.BanThoaThuan] = "Bản thỏa thuận",
        [DocumentType.GiayUyQuyen] = "Giấy ủy quyền",
        [DocumentType.GiayMoi] = "Giấy mời",
        [DocumentType.GiayGioiThieu] = "Giấy giới thiệu",
        [DocumentType.GiayNghiPhep] = "Giấy nghỉ phép",
        [DocumentType.PhieuGui] = "Phiếu gửi",
        [DocumentType.PhieuChuyen] = "Phiếu chuyển",
        [DocumentType.PhieuBao] = "Phiếu báo",
        [DocumentType.ThuCong] = "Thư công",
        [DocumentType.Khac] = "Khác",
    };

    /// <summary>
    /// Bảng chữ viết tắt tên loại VB hành chính — Theo Phụ lục III, NĐ 30/2020/NĐ-CP
    /// </summary>
    private static readonly Dictionary<DocumentType, string> _abbreviations = new()
    {
        // VBQPPL
        [DocumentType.Luat] = "Luật",
        [DocumentType.NghiDinh] = "NĐ",
        [DocumentType.ThongTu] = "TT",
        // 29 loại VB hành chính
        [DocumentType.NghiQuyet] = "NQ",
        [DocumentType.QuyetDinh] = "QĐ",
        [DocumentType.ChiThi] = "CT",
        [DocumentType.QuyChE] = "QC",
        [DocumentType.QuyDinh] = "QyĐ",
        [DocumentType.ThongCao] = "TC",
        [DocumentType.ThongBao] = "TB",
        [DocumentType.HuongDan] = "HD",
        [DocumentType.ChuongTrinh] = "CTr",
        [DocumentType.KeHoach] = "KH",
        [DocumentType.PhuongAn] = "PA",
        [DocumentType.DeAn] = "ĐA",
        [DocumentType.DuAn] = "DA",
        [DocumentType.BaoCao] = "BC",
        [DocumentType.BienBan] = "BB",
        [DocumentType.ToTrinh] = "TTr",
        [DocumentType.HopDong] = "HĐ",
        [DocumentType.CongVan] = "CV",
        [DocumentType.CongDien] = "CĐ",
        [DocumentType.BanGhiNho] = "BGN",
        [DocumentType.BanThoaThuan] = "BTT",
        [DocumentType.GiayUyQuyen] = "GUQ",
        [DocumentType.GiayMoi] = "GM",
        [DocumentType.GiayGioiThieu] = "GGT",
        [DocumentType.GiayNghiPhep] = "GNP",
        [DocumentType.PhieuGui] = "PG",
        [DocumentType.PhieuChuyen] = "PC",
        [DocumentType.PhieuBao] = "PB",
        [DocumentType.ThuCong] = "ThC",
        [DocumentType.Khac] = "",
    };

    private static readonly Dictionary<Direction, string> _dirNames = new()
    {
        [Direction.Di] = "📤 Văn bản đi",
        [Direction.Den] = "📥 Văn bản đến",
        [Direction.NoiBo] = "🔄 Nội bộ",
    };

    public static string GetDisplayName(this DocumentType type) =>
        _typeNames.TryGetValue(type, out var name) ? name : type.ToString();

    /// <summary>
    /// Lấy ký hiệu viết tắt theo Phụ lục III, NĐ 30/2020/NĐ-CP
    /// VD: DocumentType.CongVan → "CV", DocumentType.QuyetDinh → "QĐ"
    /// </summary>
    public static string GetAbbreviation(this DocumentType type) =>
        _abbreviations.TryGetValue(type, out var abbr) ? abbr : type.ToString();

    public static string GetDisplayName(this Direction dir) =>
        _dirNames.TryGetValue(dir, out var name) ? name : dir.ToString();

    public static string GetDisplayName(this UrgencyLevel level) => level switch
    {
        UrgencyLevel.Thuong => "Thường",
        UrgencyLevel.Khan => "Khẩn",
        UrgencyLevel.ThuongKhan => "Thượng khẩn",
        UrgencyLevel.HoaToc => "Hỏa tốc",
        _ => "Thường"
    };

    public static string GetDisplayName(this SecurityLevel level) => level switch
    {
        SecurityLevel.Thuong => "Thường",
        SecurityLevel.Mat => "Mật",
        SecurityLevel.ToiMat => "Tối mật",
        SecurityLevel.TuyetMat => "Tuyệt mật",
        _ => "Thường"
    };

    public static string GetDisplayName(this CopyType copyType) => copyType switch
    {
        CopyType.None => "Bản gốc",
        CopyType.SaoY => "Sao y",
        CopyType.SaoLuc => "Sao lục",
        CopyType.TrichSao => "Trích sao",
        _ => "Bản gốc"
    };

    public static string GetDisplayName(this DocumentStatus status) => status switch
    {
        DocumentStatus.Draft => "Nháp",
        DocumentStatus.PendingApproval => "Trình ký",
        DocumentStatus.Approved => "Đã duyệt",
        DocumentStatus.Signed => "Đã ký",
        DocumentStatus.Published => "Đã phát hành",
        DocumentStatus.Sent => "Đã gửi",
        DocumentStatus.Archived => "Lưu trữ",
        _ => "Nháp"
    };

    /// <summary>
    /// Ký hiệu viết tắt bản sao — Theo Phụ lục III, NĐ 30/2020/NĐ-CP
    /// </summary>
    public static string GetAbbreviation(this CopyType copyType) => copyType switch
    {
        CopyType.SaoY => "SY",
        CopyType.SaoLuc => "SL",
        CopyType.TrichSao => "TrS",
        _ => ""
    };

    public static List<KeyValuePair<CopyType, string>> GetCopyTypeItems() =>
        new List<CopyType> { CopyType.SaoY, CopyType.SaoLuc, CopyType.TrichSao }
            .Select(v => new KeyValuePair<CopyType, string>(v, v.GetDisplayName()))
            .ToList();

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

    /// <summary>
    /// Tạo danh sách {Value, Display} cho ComboBox UrgencyLevel
    /// </summary>
    public static List<KeyValuePair<UrgencyLevel, string>> GetUrgencyLevelItems() =>
        Enum.GetValues<UrgencyLevel>()
            .Select(v => new KeyValuePair<UrgencyLevel, string>(v, v.GetDisplayName()))
            .ToList();

    /// <summary>
    /// Tạo danh sách {Value, Display} cho ComboBox SecurityLevel
    /// </summary>
    public static List<KeyValuePair<SecurityLevel, string>> GetSecurityLevelItems() =>
        Enum.GetValues<SecurityLevel>()
            .Select(v => new KeyValuePair<SecurityLevel, string>(v, v.GetDisplayName()))
            .ToList();
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

/// <summary>
/// Mức độ khẩn — Theo Điều 8 khoản 3b, NĐ 30/2020/NĐ-CP
/// </summary>
public enum UrgencyLevel
{
    Thuong,         // Thường
    Khan,           // Khẩn
    ThuongKhan,     // Thượng khẩn
    HoaToc          // Hỏa tốc
}

/// <summary>
/// Độ mật — Theo Luật Bảo vệ bí mật nhà nước 2018
/// </summary>
public enum SecurityLevel
{
    Thuong,         // Thường (không mật)
    Mat,            // Mật
    ToiMat,         // Tối mật
    TuyetMat        // Tuyệt mật
}

/// <summary>
/// Loại bản sao — Theo Điều 25, NĐ 30/2020/NĐ-CP
/// Ký hiệu viết tắt theo Phụ lục III
/// </summary>
public enum CopyType
{
    None,       // Không phải bản sao (văn bản gốc)
    SaoY,       // Sao y — Ký hiệu: SY
    SaoLuc,     // Sao lục — Ký hiệu: SL
    TrichSao    // Trích sao — Ký hiệu: TrS
}
