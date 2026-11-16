namespace AIVanBan.Core.Services;

/// <summary>Lời hiện cho khách dùng chung của các hộp thoại tác vụ AI (có dấu, không đại từ nhân xưng).</summary>
public static class TacVuThongBao
{
    /// <summary>Hỏi khi đóng cửa sổ giữa lúc một lượt AI đang chạy: lượt đó không bị hủy, vẫn trừ credit.</summary>
    public const string CauHoiDongKhiDangChay =
        "Đang chạy tác vụ AI. Đóng cửa sổ thì lượt đang chạy vẫn bị trừ credit và kết quả sẽ mất. Vẫn đóng?";

    public const string TieuDeDongKhiDangChay = "Đang chạy tác vụ AI";
}
