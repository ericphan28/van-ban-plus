using System.Text.Json;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using AIVanBan.Tests.HoTro;

namespace AIVanBan.Tests;

/// <summary>Các service nghiệp vụ gửi đúng đầu vào theo hợp đồng v2 và trả lại model C# cũ.</summary>
public class TacVuDichVuTests
{
    private static TacVuClient TaoClient(HandlerTacVuGia h) => new(h, "https://vbp.thu", "khoa-thu", "1.0.20");

    private static JsonElement Than(HandlerTacVuGia h, int i = 0)
    {
        using var doc = JsonDocument.Parse(h.DaNhan[i].Body);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task TomTat_GuiDuBonTruongVaTraModelCu()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"brief":"Quyết định phê duyệt kế hoạch","key_points":[{"heading":"Điều 1","content":"Phê duyệt"}],"legal_bases":["Luật Tổ chức chính quyền địa phương"]}""");
        var dv = new DocumentSummaryService(TaoClient(h));

        var kq = await dv.SummarizeAsync("Điều 1. Phê duyệt kế hoạch.", "Quyết định", "Về kế hoạch năm 2026", "UBND xã Minh An");

        Assert.Equal("Quyết định phê duyệt kế hoạch", kq.Brief);
        Assert.Equal("Điều 1", kq.KeyPoints[0].Heading);
        Assert.Single(kq.LegalBases);
        Assert.Equal(7, dv.LuotCuoi!.CreditDaTru);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat", h.DaNhan[0].Url);
        var b = Than(h);
        Assert.Equal("Điều 1. Phê duyệt kế hoạch.", b.GetProperty("noi_dung").GetString());
        Assert.Equal("Quyết định", b.GetProperty("loai_van_ban").GetString());
        Assert.Equal("Về kế hoạch năm 2026", b.GetProperty("tieu_de").GetString());
        Assert.Equal("UBND xã Minh An", b.GetProperty("co_quan").GetString());
    }

    [Fact]
    public async Task TomTat_UocTinhCungDauVao()
    {
        var h = new HandlerTacVuGia().TraVe(System.Net.HttpStatusCode.OK,
            """{"success":true,"data":{"credit_min":4,"credit_max":13,"so_du":30,"du_credit":true}}""");

        var u = await new DocumentSummaryService(TaoClient(h)).UocTinhAsync("Điều 1.", "Quyết định");

        Assert.Equal(13, u.CreditMax);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("Điều 1.", Than(h).GetProperty("noi_dung").GetString());
    }

    [Fact]
    public async Task TomTat_NoiDungRong_KhongGoiMayChu()
    {
        var h = new HandlerTacVuGia();

        await Assert.ThrowsAsync<ArgumentException>(() => new DocumentSummaryService(TaoClient(h)).SummarizeAsync("   "));

        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task TomTat_NoiDungRong_UocTinhCungKhongGoiMayChu()
    {
        var h = new HandlerTacVuGia();

        await Assert.ThrowsAsync<ArgumentException>(() => new DocumentSummaryService(TaoClient(h)).UocTinhAsync(""));

        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task TomTat_BiCatKetQuaLaChuoiTho_NemKetQuaBiCatGiuLuotVaCredit()
    {
        // Máy chủ trả chuỗi thô (JSON cụt) thay vì đối tượng: KetQua là null, không được dùng.
        var h = new HandlerTacVuGia().ThanhCong("\"{\\\"brief\\\":\\\"Quyết định phê du\"", creditDaTru: 9, soDu: 21, biCat: true);
        var dv = new DocumentSummaryService(TaoClient(h));

        var loi = await Assert.ThrowsAsync<KetQuaBiCatException>(() => dv.SummarizeAsync("Điều 1. Nội dung dài."));

        Assert.Equal(9, loi.Luot.CreditDaTru);
        Assert.Contains("9 credit", loi.Message);
        Assert.Contains("21 credit", loi.Message);
        Assert.Contains("cắt", loi.Message);
        Assert.Equal(9, dv.LuotCuoi!.CreditDaTru);
        Assert.True(dv.LuotCuoi.BiCat);
    }

    [Fact]
    public async Task TomTat_BiCatNhungVanLaDoiTuong_TraKetQuaVaGiuCoBiCat()
    {
        var h = new HandlerTacVuGia().ThanhCong("""{"brief":"Tóm tắt đủ đọc"}""", biCat: true);
        var dv = new DocumentSummaryService(TaoClient(h));

        var kq = await dv.SummarizeAsync("Điều 1.");

        Assert.Equal("Tóm tắt đủ đọc", kq.Brief);
        Assert.True(dv.LuotCuoi!.BiCat);
    }

    [Fact]
    public async Task KiemTra_GuiVanBanDoiChieu_VaBoMarkdownTrongBanDeXuat()
    {
        var h = new HandlerTacVuGia()
            .ThanhCong("""{"overall_score":7,"summary":"Còn lỗi thể thức","issues":[{"severity":"critical","category":"format","description":"Thiếu nơi nhận"}],"suggested_content":"**Điều 1.** Nội dung"}""")
            .ThanhCong("""{"overall_score":9,"summary":"Tốt","issues":[]}""");
        var dv = new DocumentReviewService(TaoClient(h));

        var kq = await dv.ReviewDocumentAsync("Nội dung cần soát", "Công văn", "V/v báo cáo", "UBND xã Minh An", "Văn bản mẫu");

        Assert.Equal(7, kq.OverallScore);
        Assert.Equal(1, kq.CriticalCount);
        Assert.Equal("Điều 1. Nội dung", kq.SuggestedContent);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/kiem-tra", h.DaNhan[0].Url);
        var b = Than(h, 0);
        Assert.Equal("Nội dung cần soát", b.GetProperty("noi_dung").GetString());
        Assert.Equal("Công văn", b.GetProperty("loai_van_ban").GetString());
        Assert.Equal("Văn bản mẫu", b.GetProperty("van_ban_doi_chieu").GetString());

        await dv.ReviewDocumentAsync("Nội dung cần soát");
        Assert.False(Than(h, 1).TryGetProperty("van_ban_doi_chieu", out _));
    }

    [Fact]
    public async Task KiemTra_UocTinhCungDauVao_NoiDungRongKhongGoiMayChu()
    {
        var h = new HandlerTacVuGia().TraVe(System.Net.HttpStatusCode.OK,
            """{"success":true,"data":{"credit_min":5,"credit_max":20,"so_du":50,"du_credit":true}}""");
        var dv = new DocumentReviewService(TaoClient(h));

        var u = await dv.UocTinhAsync("Nội dung", "Công văn", referenceContent: "Mẫu");

        Assert.Equal(20, u.CreditMax);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/kiem-tra/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("Mẫu", Than(h).GetProperty("van_ban_doi_chieu").GetString());

        await Assert.ThrowsAsync<ArgumentException>(() => dv.ReviewDocumentAsync("  "));
        Assert.Single(h.DaNhan);
    }

    [Fact]
    public async Task KiemTra_BiCatKetQuaLaChuoiTho_NemKetQuaBiCatGiuLuot()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"{\\\"overall_score\\\":7,\\\"issues\\\":[{\\\"sever\"", creditDaTru: 12, soDu: 18, biCat: true);
        var dv = new DocumentReviewService(TaoClient(h));

        var loi = await Assert.ThrowsAsync<KetQuaBiCatException>(() => dv.ReviewDocumentAsync("Văn bản rất dài"));

        Assert.Equal(12, loi.Luot.CreditDaTru);
        Assert.Contains("kiểm tra", loi.Message);
        Assert.Equal(12, dv.LuotCuoi!.CreditDaTru);
        Assert.True(dv.LuotCuoi.BiCat);
    }

    [Fact]
    public async Task KiemTra_BiCatNhungVanLaDoiTuong_TraKetQuaVaGiuCoBiCat()
    {
        var h = new HandlerTacVuGia().ThanhCong("""{"overall_score":6,"summary":"Đủ đọc","suggested_content":"__Nội dung__"}""", biCat: true);
        var dv = new DocumentReviewService(TaoClient(h));

        var kq = await dv.ReviewDocumentAsync("Văn bản");

        Assert.Equal("Nội dung", kq.SuggestedContent);
        Assert.True(dv.LuotCuoi!.BiCat);
    }

    [Fact]
    public async Task ThamMuu_AnhXaNguCanhDungHopDong()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"summary":"UBND tỉnh yêu cầu báo cáo","urgency_level":"khan","action_items":["Bước 1: Vào sổ"],"deadlines":[{"task":"Gửi báo cáo","date":"20/11/2026"}],"response_needed":true,"priority":"high"}""");
        var ctx = new DocumentAdvisoryContext
        {
            Direction = "Đến",
            Category = "Đất đai",
            DocumentNumber = "15/UBND-KT",
            IssuedDate = new DateTime(2026, 11, 3, 14, 30, 0),
            UrgencyLevel = "Khẩn",
            SecurityLevel = "Thường",
            DueDate = new DateTime(2026, 11, 20),
            SignerName = "Nguyễn Văn A",
            SignerTitle = "CHỦ TỊCH",
            Recipients = new[] { "UBND các xã", "Lưu: VT" },
            AssignedTo = "Địa chính",
            ProcessingNotes = "Khẩn trương thực hiện",
            BasedOn = new[] { "Luật Đất đai 2024" },
            RelatedDocumentsSummary = "Công văn 10/UBND ngày 01/10/2026",
        };
        var dv = new DocumentAdvisoryService(TaoClient(h));

        var kq = await dv.AdviseAsync("Đề nghị báo cáo trước 20/11/2026.", "Công văn", "V/v báo cáo", "UBND tỉnh Đồng Nai", ctx);

        Assert.Equal("UBND tỉnh yêu cầu báo cáo", kq.Summary);
        Assert.True(kq.ResponseNeeded);
        Assert.Equal("20/11/2026", kq.Deadlines[0].Date);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tham-muu", h.DaNhan[0].Url);
        var b = Than(h);
        Assert.Equal("UBND tỉnh Đồng Nai", b.GetProperty("co_quan").GetString());
        var n = b.GetProperty("ngu_canh");
        Assert.Equal("Đến", n.GetProperty("huong").GetString());
        Assert.Equal("Đất đai", n.GetProperty("nhom").GetString());
        Assert.Equal("15/UBND-KT", n.GetProperty("so_van_ban").GetString());
        Assert.Equal("2026-11-03", n.GetProperty("ngay_ban_hanh").GetString());
        Assert.Equal("Khẩn", n.GetProperty("do_khan").GetString());
        Assert.Equal("Thường", n.GetProperty("do_mat").GetString());
        Assert.Equal("2026-11-20", n.GetProperty("han_xu_ly").GetString());
        Assert.Equal("Nguyễn Văn A", n.GetProperty("nguoi_ky").GetString());
        Assert.Equal("CHỦ TỊCH", n.GetProperty("chuc_danh_ky").GetString());
        Assert.Equal(2, n.GetProperty("noi_nhan").GetArrayLength());
        Assert.Equal("Lưu: VT", n.GetProperty("noi_nhan")[1].GetString());
        Assert.Equal("Địa chính", n.GetProperty("giao_cho").GetString());
        Assert.Equal("Khẩn trương thực hiện", n.GetProperty("ghi_chu_xu_ly").GetString());
        Assert.Equal("Luật Đất đai 2024", n.GetProperty("can_cu")[0].GetString());
        Assert.Equal("Công văn 10/UBND ngày 01/10/2026", n.GetProperty("van_ban_lien_quan").GetString());
    }

    [Fact]
    public void ThamMuu_NguCanhChiGuiTruongCoGiaTri()
    {
        Assert.Null(DocumentAdvisoryService.TaoNguCanh(null));

        var d = DocumentAdvisoryService.TaoNguCanh(new DocumentAdvisoryContext
        {
            Direction = "Đến",
            Category = "  ",
            Recipients = Array.Empty<string>(),
        })!;

        Assert.Equal(new[] { "huong" }, d.Keys.ToArray());
    }

    [Fact]
    public void ThamMuu_NguCanhBoPhanTuMangRongHoacNull()
    {
        var d = DocumentAdvisoryService.TaoNguCanh(new DocumentAdvisoryContext
        {
            Recipients = new[] { "UBND các xã", null!, "  " },
            BasedOn = new string[] { null! },
        })!;

        Assert.Equal(new[] { "noi_nhan" }, d.Keys.ToArray());
        Assert.Equal(new[] { "UBND các xã" }, (string[])d["noi_nhan"]);
    }

    [Fact]
    public async Task ThamMuu_NgayLuonDangIsoDuLocaleMayLaGi()
    {
        var cu = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");
            var d = DocumentAdvisoryService.TaoNguCanh(new DocumentAdvisoryContext { IssuedDate = new DateTime(2026, 1, 5), DueDate = new DateTime(2026, 12, 31) })!;
            Assert.Equal("2026-01-05", d["ngay_ban_hanh"]);
            Assert.Equal("2026-12-31", d["han_xu_ly"]);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = cu;
        }
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ThamMuu_KhongCoNguCanh_KhongGuiTruong()
    {
        var h = new HandlerTacVuGia().ThanhCong("""{"summary":"Tóm tắt"}""");

        await new DocumentAdvisoryService(TaoClient(h)).AdviseAsync("Nội dung văn bản đến");

        Assert.False(Than(h).TryGetProperty("ngu_canh", out _));
    }

    [Fact]
    public async Task ThamMuu_UocTinhCungDauVao_NoiDungRongKhongGoiMayChu()
    {
        var h = new HandlerTacVuGia().TraVe(System.Net.HttpStatusCode.OK,
            """{"success":true,"data":{"credit_min":3,"credit_max":9,"so_du":40,"du_credit":true}}""");
        var dv = new DocumentAdvisoryService(TaoClient(h));

        var u = await dv.UocTinhAsync("Nội dung", context: new DocumentAdvisoryContext { Direction = "Đến" });

        Assert.Equal(9, u.CreditMax);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tham-muu/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("Đến", Than(h).GetProperty("ngu_canh").GetProperty("huong").GetString());

        await Assert.ThrowsAsync<ArgumentException>(() => dv.AdviseAsync(""));
        Assert.Single(h.DaNhan);
    }

    [Fact]
    public async Task ThamMuu_BiCatKetQuaLaChuoiTho_NemKetQuaBiCatGiuLuot()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"{\\\"summary\\\":\\\"UBND tỉnh yêu\"", creditDaTru: 6, soDu: 24, biCat: true);
        var dv = new DocumentAdvisoryService(TaoClient(h));

        var loi = await Assert.ThrowsAsync<KetQuaBiCatException>(() => dv.AdviseAsync("Văn bản đến"));

        Assert.Equal(6, loi.Luot.CreditDaTru);
        Assert.Contains("tham mưu", loi.Message);
        Assert.Equal(6, dv.LuotCuoi!.CreditDaTru);
        Assert.True(dv.LuotCuoi.BiCat);
    }
}
