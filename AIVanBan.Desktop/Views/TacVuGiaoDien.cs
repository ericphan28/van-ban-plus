using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using AIVanBan.Core.Services;

namespace AIVanBan.Desktop.Views;

/// <summary>
/// Giao diện dùng chung cho tác vụ AI v2: báo trước khoảng credit và số dư, báo lỗi theo mã,
/// mời mua credit khi thiếu. Lời hiện cho khách: không đại từ nhân xưng, không «dạ/vâng/ạ».
/// </summary>
public static class TacVuGiaoDien
{
    /// <summary>
    /// Gọi /uoc-tinh rồi hỏi xác nhận. Trả true khi được phép chạy. Thiếu credit thì mời mua và trả false;
    /// lỗi khi ước tính thì báo lỗi và trả false.
    /// </summary>
    public static async Task<bool> XacNhanCreditAsync(Window owner, string tenViec,
        Func<Task<UocTinhKetQua>> uocTinh, string? moTaThem = null)
    {
        UocTinhKetQua? uoc = null;
        Exception? loi = null;
        Mouse.OverrideCursor = Cursors.Wait;
        try
        {
            uoc = await uocTinh();
        }
        catch (Exception ex)
        {
            loi = ex;
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }

        if (loi != null || uoc == null)
        {
            HienLoi(owner, loi ?? new InvalidOperationException("Không ước tính được credit."), tenViec);
            return false;
        }

        if (!uoc.DuCredit)
        {
            MoiMuaCredit(owner,
                $"{tenViec} dự kiến dùng {KhoangCredit(uoc)}, số dư hiện có {uoc.SoDu} credit: không đủ để chạy. Credit chưa bị trừ.");
            return false;
        }

        var noiDung = (string.IsNullOrWhiteSpace(moTaThem) ? "" : moTaThem.TrimEnd() + "\n\n")
            + $"💳 {tenViec} dự kiến dùng {KhoangCredit(uoc)}.\n"
            + $"Số dư hiện có: {uoc.SoDu} credit.\n\n"
            + "Credit trừ theo mức dùng thật, không vượt mức trên; tác vụ lỗi được hoàn credit.\n\n"
            + "Tiếp tục?";
        return MessageBox.Show(owner, noiDung, "Xác nhận dùng credit",
            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    public static string KhoangCredit(UocTinhKetQua u) =>
        u.CreditMin == u.CreditMax ? $"{u.CreditMax} credit" : $"{u.CreditMin}–{u.CreditMax} credit";

    /// <summary>Lời báo lỗi để hiện trong khung lỗi của hộp thoại.</summary>
    public static string MoTaLoi(Exception ex) => ex switch
    {
        TacVuException t => t.Message,
        KetQuaBiCatException c => c.Message,
        ArgumentException a => a.Message,
        _ => $"Lỗi: {ex.Message}",
    };

    /// <summary>Báo lỗi bằng hộp thoại; thiếu credit thì mời mua credit.</summary>
    public static void HienLoi(Window owner, Exception ex, string tieuDe)
    {
        if (ex is TacVuException { Ma: MaLoiTacVu.KhongDuCredit })
        {
            MoiMuaCredit(owner, ex.Message);
            return;
        }
        MessageBox.Show(owner, MoTaLoi(ex), tieuDe, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>Cho hộp thoại tự hiện lỗi trong khung: chỉ bật lời mời mua khi lỗi là thiếu credit.</summary>
    public static void MoiMuaCreditNeuThieu(Window owner, Exception ex)
    {
        if (ex is TacVuException { Ma: MaLoiTacVu.KhongDuCredit })
            MoiMuaCredit(owner, ex.Message);
    }

    public static void MoiMuaCredit(Window owner, string loiDan)
    {
        var url = DiaChiMuaCredit();
        var chon = MessageBox.Show(owner, $"{loiDan}\n\nMở trang mua credit?\n{url}", "Không đủ credit",
            MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (chon != MessageBoxResult.Yes) return;
        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, $"Không mở được trình duyệt. Địa chỉ trang mua credit: {url}\n\n{ex.Message}",
                "Mua credit", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public static string DiaChiMuaCredit()
    {
        var goc = AppSettingsService.Load().VanBanPlusApiUrl;
        if (string.IsNullOrWhiteSpace(goc)) goc = "https://vanbanplus.giakiemso.com";
        return goc.Trim().TrimEnd('/') + "/bang-gia";
    }

    /// <summary>Kết quả chạm trần độ dài thì báo (vẫn tính là thành công, có trừ credit — spec §6).</summary>
    public static void CanhBaoNeuBiCat(Window owner, ThongTinLuot? luot)
    {
        if (luot?.BiCat == true)
        {
            MessageBox.Show(owner,
                "Kết quả dài vượt giới hạn nên bị cắt bớt ở cuối. Nên kiểm tra phần cuối trước khi dùng.",
                "Kết quả bị cắt", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
