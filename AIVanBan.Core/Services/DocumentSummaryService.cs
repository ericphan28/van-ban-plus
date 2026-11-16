using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tóm tắt văn bản nhà nước bằng AI.
/// Prompt được thiết kế khoa học, logic, phù hợp với cấu trúc văn bản hành chính Việt Nam.
/// </summary>
public class DocumentSummaryService
{
    private readonly TacVuClient _tacVu;

    public DocumentSummaryService() : this(new TacVuClient()) { }

    public DocumentSummaryService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    /// <summary>Khoảng credit dự kiến cho đúng đầu vào sẽ gửi (không gọi AI, không giữ credit).</summary>
    public Task<UocTinhKetQua> UocTinhAsync(string content, string documentType = "", string title = "", string issuer = "")
        => _tacVu.UocTinhAsync("tom-tat", DauVao(content, documentType, title, issuer));

    /// <summary>
    /// Tóm tắt văn bản qua tác vụ v2 tom-tat. Kết quả bị cắt mà máy chủ trả chuỗi thô:
    /// ném <see cref="KetQuaBiCatException"/> (credit đã trừ, <see cref="LuotCuoi"/> vẫn được ghi).
    /// </summary>
    public async Task<DocumentSummary> SummarizeAsync(
        string content,
        string documentType = "",
        string title = "",
        string issuer = "")
    {
        var kq = await _tacVu.ChayAsync<DocumentSummary>("tom-tat", DauVao(content, documentType, title, issuer));
        LuotCuoi = kq.Luot;
        if (kq.ChuoiTho != null || kq.KetQua is null)
            throw new KetQuaBiCatException("tóm tắt", kq.Luot);
        return kq.KetQua;
    }

    private static object DauVao(string content, string documentType, string title, string issuer)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung văn bản không được để trống.");
        return new { noi_dung = content, loai_van_ban = documentType, tieu_de = title, co_quan = issuer };
    }
}
