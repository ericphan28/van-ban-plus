using System.Globalization;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tham mưu xử lý văn bản đến bằng AI.
/// Phân tích nội dung → tóm tắt, đề xuất xử lý, deadline, dự thảo phản hồi.
/// </summary>
public class DocumentAdvisoryService
{
    private readonly TacVuClient _tacVu;

    public DocumentAdvisoryService() : this(new TacVuClient()) { }

    public DocumentAdvisoryService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    /// <summary>Khoảng credit dự kiến cho đúng đầu vào sẽ gửi (không gọi AI, không giữ credit).</summary>
    public Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "",
        string issuer = "", DocumentAdvisoryContext? context = null)
        => _tacVu.UocTinhAsync("tham-muu", DauVao(content, documentType, title, issuer, context));

    /// <summary>
    /// Phân tích văn bản đến và tham mưu xử lý qua tác vụ v2 tham-muu. Kết quả bị cắt mà máy chủ trả chuỗi thô:
    /// ném <see cref="KetQuaBiCatException"/> (credit đã trừ, <see cref="LuotCuoi"/> vẫn được ghi).
    /// </summary>
    public async Task<DocumentAdvisory> AdviseAsync(
        string content,
        string documentType = "",
        string title = "",
        string issuer = "",
        DocumentAdvisoryContext? context = null)
    {
        var kq = await _tacVu.ChayAsync<DocumentAdvisory>("tham-muu", DauVao(content, documentType, title, issuer, context));
        LuotCuoi = kq.Luot;
        if (kq.ChuoiTho != null || kq.KetQua is null)
            throw new KetQuaBiCatException("tham mưu", kq.Luot);
        return kq.KetQua;
    }

    private static object DauVao(string content, string documentType, string title, string issuer, DocumentAdvisoryContext? context)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung văn bản không được để trống.");
        return new
        {
            noi_dung = content,
            loai_van_ban = documentType,
            tieu_de = title,
            co_quan = issuer,
            ngu_canh = TaoNguCanh(context),
        };
    }

    /// <summary>
    /// DocumentAdvisoryContext → ngu_canh theo hợp đồng v2. Ngày gửi dạng ISO yyyy-MM-dd.
    /// Chỉ gửi trường có giá trị (chuỗi không trắng, mảng có ít nhất một phần tử không trắng).
    /// </summary>
    internal static Dictionary<string, object>? TaoNguCanh(DocumentAdvisoryContext? c)
    {
        if (c == null) return null;
        var d = new Dictionary<string, object>();
        void Chuoi(string khoa, string? v) { if (!string.IsNullOrWhiteSpace(v)) d[khoa] = v; }
        void Ngay(string khoa, DateTime? v) { if (v.HasValue) d[khoa] = v.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); }
        void Mang(string khoa, string[]? v)
        {
            var co = v?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (co is { Length: > 0 }) d[khoa] = co;
        }

        Chuoi("huong", c.Direction);
        Chuoi("nhom", c.Category);
        Chuoi("so_van_ban", c.DocumentNumber);
        Ngay("ngay_ban_hanh", c.IssuedDate);
        Chuoi("do_khan", c.UrgencyLevel);
        Chuoi("do_mat", c.SecurityLevel);
        Ngay("han_xu_ly", c.DueDate);
        Chuoi("nguoi_ky", c.SignerName);
        Chuoi("chuc_danh_ky", c.SignerTitle);
        Mang("noi_nhan", c.Recipients);
        Chuoi("giao_cho", c.AssignedTo);
        Chuoi("ghi_chu_xu_ly", c.ProcessingNotes);
        Mang("can_cu", c.BasedOn);
        Chuoi("van_ban_lien_quan", c.RelatedDocumentsSummary);
        return d;
    }
}

/// <summary>
/// Dữ liệu ngữ cảnh bổ sung cho AI Tham mưu — trích từ Document model.
/// Giúp AI phân tích chính xác hơn thay vì phải "đoán" từ nội dung.
/// </summary>
public class DocumentAdvisoryContext
{
    /// <summary>Hướng VB: Đến / Đi / Nội bộ — quyết định cách tham mưu</summary>
    public string? Direction { get; set; }
    
    /// <summary>Lĩnh vực (Category) — giúp phân công đúng người</summary>
    public string? Category { get; set; }
    
    /// <summary>Số ký hiệu VB (VD: 123/CV-UBND)</summary>
    public string? DocumentNumber { get; set; }
    
    /// <summary>Ngày ban hành</summary>
    public DateTime? IssuedDate { get; set; }
    
    /// <summary>Mức độ khẩn (display name: Thường/Khẩn/Thượng khẩn/Hỏa tốc)</summary>
    public string? UrgencyLevel { get; set; }
    
    /// <summary>Độ mật (display name: Thường/Mật/Tối mật/Tuyệt mật)</summary>
    public string? SecurityLevel { get; set; }
    
    /// <summary>Hạn xử lý</summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>Người ký văn bản</summary>
    public string? SignerName { get; set; }
    
    /// <summary>Chức danh người ký</summary>
    public string? SignerTitle { get; set; }
    
    /// <summary>Nơi nhận</summary>
    public string[]? Recipients { get; set; }
    
    /// <summary>Người/đơn vị được phân công xử lý</summary>
    public string? AssignedTo { get; set; }
    
    /// <summary>Ý kiến chỉ đạo</summary>
    public string? ProcessingNotes { get; set; }
    
    /// <summary>Các căn cứ pháp lý viện dẫn trong VB</summary>
    public string[]? BasedOn { get; set; }
    
    /// <summary>Tóm tắt VB liên quan trước đó (nếu có)</summary>
    public string? RelatedDocumentsSummary { get; set; }

    /// <summary>
    /// Tạo context từ Document model
    /// </summary>
    public static DocumentAdvisoryContext FromDocument(Document doc)
    {
        return new DocumentAdvisoryContext
        {
            Direction = doc.Direction.GetDisplayName(),
            Category = doc.Category,
            DocumentNumber = doc.Number,
            IssuedDate = doc.IssueDate,
            UrgencyLevel = doc.UrgencyLevel.GetDisplayName(),
            SecurityLevel = doc.SecurityLevel.GetDisplayName(),
            DueDate = doc.DueDate,
            SignerName = doc.SignedBy,
            SignerTitle = doc.SigningTitle,
            Recipients = doc.Recipients,
            AssignedTo = doc.AssignedTo,
            ProcessingNotes = doc.ProcessingNotes,
            BasedOn = doc.BasedOn,
        };
    }
}
