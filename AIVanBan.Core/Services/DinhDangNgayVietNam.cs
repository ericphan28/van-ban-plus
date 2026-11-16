using System.Globalization;

namespace AIVanBan.Core.Services;

/// <summary>
/// Ngày tháng kiểu Việt Nam (dd/MM/yyyy) cho toàn ứng dụng. WPF DatePicker đọc chữ gõ tay theo định dạng ngày của
/// culture hiện hành: máy để vùng en-US thì gõ «05/02/2026» bị lưu thành ngày 02/05/2026 (lỗi phát hiện 16/11/2026).
/// Chỉ thay phần ngày giờ bằng vi-VN, GIỮ NGUYÊN định dạng số của máy để các chỗ đọc số từ chuỗi không đổi hành vi.
/// </summary>
public static class DinhDangNgayVietNam
{
    public const string MauNgay = "dd/MM/yyyy";

    public static CultureInfo TaoCulture(CultureInfo goc)
    {
        var c = (CultureInfo)goc.Clone();
        var ngay = (DateTimeFormatInfo)CultureInfo.GetCultureInfo("vi-VN").DateTimeFormat.Clone();
        ngay.ShortDatePattern = MauNgay;
        ngay.DateSeparator = "/";
        c.DateTimeFormat = ngay;
        return c;
    }

    /// <summary>Gọi một lần lúc khởi động, trước khi tạo cửa sổ đầu tiên.</summary>
    public static void ApDung()
    {
        var c = TaoCulture(CultureInfo.CurrentCulture);
        CultureInfo.CurrentCulture = c;
        CultureInfo.DefaultThreadCurrentCulture = c;
    }
}
