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
    /// <summary>Đi ngược từ thư mục build lên tới gốc repo (nơi có vanbanplus-api/package.json); không thấy thì báo lỗi.</summary>
    private static string TimThuMucMau()
    {
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d != null; d = d.Parent)
        {
            if (File.Exists(Path.Combine(d.FullName, "vanbanplus-api", "package.json")))
                return Path.Combine(d.FullName, "vanbanplus-api", "tests", "mau-prompt");
        }
        throw new DirectoryNotFoundException(
            $"Không tìm thấy vanbanplus-api/package.json khi đi ngược từ {AppContext.BaseDirectory}");
    }

    private static string ChuanHoa(string s) => s.Replace("\r\n", "\n");

    /// <summary>
    /// Ghi tệp mẫu. Nội dung khác bản đang có thì vẫn ghi bản mới nhưng làm hỏng bài kiểm,
    /// để nhớ commit tệp mẫu mới bên vanbanplus-api (bộ kiểm TypeScript đọc tệp đã commit).
    /// </summary>
    private static void Ghi(string ten, object duLieu)
    {
        var thuMuc = TimThuMucMau();
        Directory.CreateDirectory(thuMuc);
        var duongDan = Path.Combine(thuMuc, ten);
        var json = JsonSerializer.Serialize(duLieu, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }).Replace("\r\n", "\n") + "\n";
        var cu = File.Exists(duongDan) ? File.ReadAllText(duongDan).Replace("\r\n", "\n") : null;
        if (cu == json) return;
        File.WriteAllText(duongDan, json);
        Assert.Fail($"Tệp mẫu {duongDan} vừa được sinh lại vì prompt C# đổi: commit tệp này bên vanbanplus-api.");
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
    }
}
