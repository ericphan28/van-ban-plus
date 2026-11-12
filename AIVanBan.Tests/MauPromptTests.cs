using System.Text.Encodings.Web;
using System.Text.Json;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

/// <summary>
/// Sinh tệp mẫu prompt cho vanbanplus-api (tests/mau-prompt/*.json): đầu vào + system prompt + user prompt
/// do chính mã C# dựng. Bộ kiểm TypeScript so từng ký tự với prompt dựng ở máy chủ.
/// Xuống dòng chuẩn hóa về \n vì chuỗi @"..." mang CRLF của tệp nguồn khi build trên Windows.
/// </summary>
public class MauPromptTests
{
    private static readonly string ThuMucMau = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "vanbanplus-api", "tests", "mau-prompt"));

    private static string ChuanHoa(string s) => s.Replace("\r\n", "\n");

    private static void Ghi(string ten, object duLieu)
    {
        Directory.CreateDirectory(ThuMucMau);
        var json = JsonSerializer.Serialize(duLieu, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        });
        File.WriteAllText(Path.Combine(ThuMucMau, ten), json.Replace("\r\n", "\n") + "\n");
    }

    [Fact]
    public void GhiMauTomTat()
    {
        var cacMau = new[]
        {
            (NoiDung: "Điều 1. Phê duyệt kế hoạch.\nĐiều 2. Giao Văn phòng theo dõi.", Loai: "Quyết định", TieuDe: "Về việc phê duyệt kế hoạch năm 2026", CoQuan: "UBND xã Minh An"),
            (NoiDung: "Kính gửi: Các phòng chuyên môn.\nĐề nghị báo cáo trước ngày 20/11/2026.", Loai: "", TieuDe: "", CoQuan: ""),
            (NoiDung: "Nội dung có \"ngoặc kép\", dấu \\ và ký tự ```.", Loai: "Công văn", TieuDe: "", CoQuan: "Sở Nội vụ"),
        };

        var mau = cacMau.Select(m => new
        {
            dau_vao = new { noi_dung = m.NoiDung, loai_van_ban = m.Loai, tieu_de = m.TieuDe, co_quan = m.CoQuan },
            he_thong = ChuanHoa(DocumentSummaryService.BuildSystemPrompt()),
            nguoi_dung = ChuanHoa(DocumentSummaryService.BuildUserPrompt(m.NoiDung, m.Loai, m.TieuDe, m.CoQuan)),
        }).ToList();

        Ghi("tom-tat.json", mau);
        Assert.True(File.Exists(Path.Combine(ThuMucMau, "tom-tat.json")));
    }
}
