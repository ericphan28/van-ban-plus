namespace AIVanBan.Core.Services;

/// <summary>
/// Kết quả chạm trần độ dài và máy chủ trả chuỗi thô, không đọc được thành mô hình kết quả.
/// Lượt chạy vẫn tính là thành công nên credit đã trừ (spec §6); <see cref="Exception.Message"/> là lời hiện cho khách.
/// </summary>
public sealed class KetQuaBiCatException : Exception
{
    public KetQuaBiCatException(string tenViec, ThongTinLuot luot)
        : base($"Kết quả {tenViec} dài vượt giới hạn nên bị cắt ở cuối và không đọc được thành kết quả hoàn chỉnh. "
             + $"Đã trừ {luot.CreditDaTru} credit, số dư còn {luot.SoDu} credit. "
             + "Nên chia nội dung thành các phần ngắn hơn rồi chạy lại.")
    {
        Luot = luot;
    }

    public ThongTinLuot Luot { get; }
}
