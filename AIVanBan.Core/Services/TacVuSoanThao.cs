using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Soạn văn bản qua tác vụ v2 soan-thao. Prompt (26 nhánh theo loại văn bản) và năm hiện hành nằm ở máy chủ;
/// bản cài máy chỉ gửi loại văn bản (tên enum DocumentType), tên mẫu, các trường người dùng nhập (theo thứ tự ô nhập)
/// và địa phương của cơ quan.
/// </summary>
public sealed class TacVuSoanThao
{
    private readonly TacVuClient _tacVu;

    public TacVuSoanThao() : this(new TacVuClient()) { }

    public TacVuSoanThao(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    public Task<UocTinhKetQua> UocTinhAsync(DocumentType loai, IReadOnlyDictionary<string, string> truong,
        DiaPhuongCoQuan? diaPhuong, string? tenMau = null)
        => _tacVu.UocTinhAsync("soan-thao", DauVao(loai, truong, diaPhuong, tenMau));

    public async Task<string> SoanAsync(DocumentType loai, IReadOnlyDictionary<string, string> truong,
        DiaPhuongCoQuan? diaPhuong, string? tenMau = null)
    {
        var kq = await _tacVu.ChayAsync<string>("soan-thao", DauVao(loai, truong, diaPhuong, tenMau));
        LuotCuoi = kq.Luot;
        return kq.KetQua ?? "";
    }

    private static object DauVao(DocumentType loai, IReadOnlyDictionary<string, string> truong, DiaPhuongCoQuan? dp, string? tenMau) => new
    {
        loai_van_ban = loai.ToString(),
        ten_mau = string.IsNullOrWhiteSpace(tenMau) ? null : tenMau.Trim(),
        truong,
        dia_phuong = dp is null ? null : new
        {
            xa = dp.Xa,
            co_quan_cap_tren = dp.CoQuanCapTren,
            tinh = dp.Tinh,
            dac_diem = dp.DacDiem.Count == 0 ? null : dp.DacDiem,
        },
    };
}
