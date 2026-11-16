namespace AIVanBan.Core.Services;

/// <summary>Lời hiện cho khách dùng chung của các hộp thoại tác vụ AI (có dấu, không đại từ nhân xưng).</summary>
public static class TacVuThongBao
{
    /// <summary>Hỏi khi đóng cửa sổ giữa lúc một lượt AI đang chạy: lượt đó không bị hủy, vẫn trừ credit.</summary>
    public const string CauHoiDongKhiDangChay =
        "Đang chạy tác vụ AI. Đóng cửa sổ thì lượt đang chạy vẫn bị trừ credit và kết quả sẽ mất. Vẫn đóng?";

    public const string TieuDeDongKhiDangChay = "Đang chạy tác vụ AI";

    /// <summary>Hỏi khi đóng cửa sổ giữa lúc đang ước tính credit: lượt chưa chạy nên không bị trừ.</summary>
    public const string CauHoiDongKhiUocTinh =
        "Đang ước tính credit. Đóng cửa sổ thì tác vụ sẽ không chạy và không bị trừ credit. Vẫn đóng?";

    /// <summary>Chọn lời hỏi theo pha: đang ước tính (chưa trừ credit) hay đang chạy (đã giữ credit).</summary>
    public static string CauHoiDong(bool dangUocTinh) => dangUocTinh ? CauHoiDongKhiUocTinh : CauHoiDongKhiDangChay;
}
