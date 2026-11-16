using System.Text.Encodings.Web;
using System.Text.Json;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// Bằng chứng tách prompt soạn thảo sang AIVanBan.Core không đổi hành vi (spec mục 10).
/// Bản chụp MauCu/soan-thao-gia-kiem.json là prompt của hộp thoại CŨ (chụp ở đợt 2b); lớp mới phải khớp từng ký tự.
/// Từ đợt 3 hộp thoại không còn dựng prompt (gọi tác vụ soan-thao v2), nên chỉ còn so lớp mới với bản chụp.
/// KHÔNG xóa / sinh lại bản chụp.
/// </summary>
public class SoanThaoTuongDuongTests
{
    private static readonly JsonSerializerOptions TUY_CHON = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    internal sealed record Chup(string ten, string he_thong, string nguoi_dung);

    internal static string TepChup()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        {
            if (File.Exists(Path.Combine(d.FullName, "AIVanBan.sln")))
                return Path.Combine(d.FullName, "AIVanBan.Tests", "MauCu", "soan-thao-gia-kiem.json");
        }
        throw new DirectoryNotFoundException($"Không tìm thấy AIVanBan.sln khi đi ngược từ {AppContext.BaseDirectory}");
    }

    internal static string ChuanHoa(string s) => s.Replace("\r\n", "\n");

    internal static List<Chup> DocChup() =>
        JsonSerializer.Deserialize<List<Chup>>(File.ReadAllText(TepChup())) ?? throw new InvalidDataException("Bản chụp rỗng");

    [Fact]
    public void LopMoiKhopBanChup()
    {
        var chup = DocChup();
        Assert.Equal(chup.Select(c => c.ten), MauSoanThao.GiaKiem.Select(c => c.Ten));
        foreach (var (ca, c) in MauSoanThao.GiaKiem.Zip(chup))
        {
            var h = ChuanHoa(ComposePromptBuilder.BuildSystemInstruction(ca.Loai, ca.DiaPhuong, 2026));
            var n = ChuanHoa(ComposePromptBuilder.BuildUserPrompt(ca.Loai, ca.TenMau, ca.TuDien(), ca.DiaPhuong));
            Assert.True(c.he_thong == h, $"{ca.Ten}: he_thong của lớp mới lệch hộp thoại cũ");
            Assert.True(c.nguoi_dung == n, $"{ca.Ten}: nguoi_dung của lớp mới lệch hộp thoại cũ");
        }
    }
}
