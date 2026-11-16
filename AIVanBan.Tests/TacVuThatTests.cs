using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using Xunit.Abstractions;

namespace AIVanBan.Tests;

/// <summary>
/// Chạy thật 6 tác vụ v2 trên production bằng tài khoản thử (tốn credit thật, khoảng 60–150 credit một lượt).
/// Chỉ chạy khi có biến môi trường VBP_E2E_API_KEY; không có thì in «BỎ QUA» và kết thúc (bộ kiểm thường vẫn xanh).
/// </summary>
public class TacVuThatTests
{
    private readonly ITestOutputHelper _out;

    public TacVuThatTests(ITestOutputHelper output) => _out = output;

    private TacVuClient? TaoClient()
    {
        var khoa = Environment.GetEnvironmentVariable("VBP_E2E_API_KEY");
        if (string.IsNullOrWhiteSpace(khoa))
        {
            _out.WriteLine("BỎ QUA: thiếu VBP_E2E_API_KEY");
            return null;
        }
        var diaChi = Environment.GetEnvironmentVariable("VBP_E2E_URL") ?? "https://vanbanplus.giakiemso.com";
        return new TacVuClient(new HttpClientHandler(), diaChi, khoa.Trim(), "1.0.20");
    }

    private void Ghi(string ten, UocTinhKetQua u, ThongTinLuot? l)
    {
        Assert.NotNull(l);
        _out.WriteLine($"OK {ten}: uoc-tinh {u.CreditMin}-{u.CreditMax}, tru {l!.CreditDaTru}, so du {l.SoDu}, bi_cat {l.BiCat}, ma_luot {l.MaLuot}");
        Assert.InRange(l.CreditDaTru, 1, u.CreditMax);
    }

    private static string TaoAnhGiayMoi()
    {
        using var bmp = new Bitmap(1240, 900);
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var font = new Font("Times New Roman", 26);
            var dong = new[]
            {
                "UBND XÃ MINH AN", "Số: 15/GM-UBND", "GIẤY MỜI", "Về việc họp giao ban tháng 11/2026",
                "Thời gian: 8 giờ 00, ngày 20/11/2026", "Địa điểm: Hội trường UBND xã Minh An",
                "CHỦ TỊCH", "Nguyễn Văn An",
            };
            for (var i = 0; i < dong.Length; i++)
                g.DrawString(dong[i], font, Brushes.Black, 60, 50 + i * 95);
        }
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return Convert.ToBase64String(ms.ToArray());
    }

    [Fact]
    public async Task SauTacVu_ChayThatTrenProduction()
    {
        var c = TaoClient();
        if (c == null) return;

        var tomTat = new DocumentSummaryService(c);
        const string qd = "Điều 1. Phê duyệt kế hoạch chuyển đổi số năm 2026 của UBND xã Minh An.\nĐiều 2. Giao Văn phòng theo dõi, báo cáo trước 30/12/2026.";
        var u = await tomTat.UocTinhAsync(qd, "Quyết định", "Phê duyệt kế hoạch chuyển đổi số", "UBND xã Minh An");
        var s = await tomTat.SummarizeAsync(qd, "Quyết định", "Phê duyệt kế hoạch chuyển đổi số", "UBND xã Minh An");
        Assert.False(string.IsNullOrWhiteSpace(s.Brief));
        Ghi("tom-tat", u, tomTat.LuotCuoi);

        var kiemTra = new DocumentReviewService(c);
        const string cv = "Kính gửi: Phòng Văn hóa.\nUBND xã Minh An đề nghị cung cấp số liệu về du lịch trươc ngày 20/11/2026.\nTrân trọng cảm ơn.";
        u = await kiemTra.UocTinhAsync(cv, "Công văn", "V/v cung cấp số liệu", "UBND xã Minh An");
        var r = await kiemTra.ReviewDocumentAsync(cv, "Công văn", "V/v cung cấp số liệu", "UBND xã Minh An");
        Assert.False(string.IsNullOrWhiteSpace(r.Summary));
        Ghi("kiem-tra", u, kiemTra.LuotCuoi);

        var thamMuu = new DocumentAdvisoryService(c);
        var ctx = new DocumentAdvisoryContext
        {
            Direction = "Đến", DocumentNumber = "120/UBND-VX", IssuedDate = new DateTime(2026, 11, 10),
            DueDate = new DateTime(2026, 11, 25), UrgencyLevel = "Khẩn", Recipients = new[] { "UBND các xã" },
        };
        const string den = "UBND tỉnh đề nghị UBND các xã báo cáo kết quả chuyển đổi số năm 2026 trước ngày 25/11/2026.";
        u = await thamMuu.UocTinhAsync(den, "Công văn", "V/v báo cáo chuyển đổi số", "UBND tỉnh Đồng Nai", ctx);
        var a = await thamMuu.AdviseAsync(den, "Công văn", "V/v báo cáo chuyển đổi số", "UBND tỉnh Đồng Nai", ctx);
        Assert.False(string.IsNullOrWhiteSpace(a.Summary));
        Ghi("tham-muu", u, thamMuu.LuotCuoi);

        var baoCao = new PeriodicReportService(c);
        const string soLieu = "Thu ngân sách: 1,2 tỷ đồng (kỳ trước 1,0 tỷ đồng).\nCấp 25 giấy chứng nhận quyền sử dụng đất.";
        u = await baoCao.UocTinhAsync("Tháng", "Tháng 10/2026", "Kinh tế - Xã hội", "UBND xã Minh An", soLieu, null, "Nguyễn Văn An", "CHỦ TỊCH");
        var bc = await baoCao.GenerateReportAsync("Tháng", "Tháng 10/2026", "Kinh tế - Xã hội", "UBND xã Minh An", soLieu, null, "Nguyễn Văn An", "CHỦ TỊCH");
        Assert.True(bc.Length > 100);
        Ghi("bao-cao-dinh-ky", u, baoCao.LuotCuoi);

        var trichXuat = new TrichXuatService(c);
        var anh = new[] { (TaoAnhGiayMoi(), "image/png") };
        u = await trichXuat.UocTinhAsync(anh);
        var x = await trichXuat.ExtractDocumentFromMultipleFilesAsync(anh);
        Assert.Contains("15", x.SoVanBan);
        Ghi("trich-xuat cau-truc", u, trichXuat.LuotCuoi);
        u = await trichXuat.UocTinhAsync(anh, chiChu: true);
        var chu = await trichXuat.ReadTextAsync(anh[0].Item1, "image/png");
        Assert.Contains("giấy mời", chu.ToLowerInvariant());
        Ghi("trich-xuat chi-chu", u, trichXuat.LuotCuoi);

        var soanThao = new TacVuSoanThao(c);
        var truong = new Dictionary<string, string>
        {
            ["from_org"] = "UBND xã Minh An",
            ["to_org"] = "UBND tỉnh Đồng Nai",
            ["subject"] = "Báo cáo công tác chuẩn bị mùa mưa bão năm 2026",
            ["content"] = "Báo cáo ngắn về công tác chuẩn bị phòng chống thiên tai mùa mưa bão năm 2026.",
            ["signer_name"] = "Nguyễn Văn An",
            ["signer_title"] = "CHỦ TỊCH",
        };
        var dp = new DiaPhuongCoQuan("xã Minh An", "tỉnh Đồng Nai", "tỉnh Đồng Nai");
        u = await soanThao.UocTinhAsync(DocumentType.CongVan, truong, dp);
        var vb = await soanThao.SoanAsync(DocumentType.CongVan, truong, dp);
        Assert.Contains("Minh An", vb);
        Assert.DoesNotContain("Gia Kiệm", vb);
        Ghi("soan-thao", u, soanThao.LuotCuoi);
    }
}
