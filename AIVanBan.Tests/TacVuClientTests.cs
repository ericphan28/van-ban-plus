using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIVanBan.Core.Models;
using AIVanBan.Core.Services;
using AIVanBan.Tests.HoTro;

namespace AIVanBan.Tests;

public class TacVuClientTests
{
    private static TacVuClient TaoClient(HandlerTacVuGia h, TimeSpan? han = null, string khoa = "khoa-thu") =>
        new(h, "https://vbp.thu", khoa, "1.0.20", han);

    private static JsonElement Than(HandlerTacVuGia h, int i = 0)
    {
        using var doc = JsonDocument.Parse(h.DaNhan[i].Body);
        return doc.RootElement.Clone();
    }

    [Fact]
    public async Task ChayAsync_GuiDungDuongDanHeaderVaBoTruongNull()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"kết quả\"");

        await TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "Điều 1.", tieu_de = (string?)null });

        var y = Assert.Single(h.DaNhan);
        Assert.Equal(HttpMethod.Post, y.Method);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat", y.Url);
        Assert.Equal("khoa-thu", y.Header["X-API-Key"]);
        Assert.Equal("desktop", y.Header["X-VBP-Nguon"]);
        Assert.Equal("1.0.20", y.Header["X-VBP-Phien-Ban"]);
        Assert.StartsWith("application/json", y.Header["Content-Type"]);
        var b = Than(h);
        Assert.Equal("Điều 1.", b.GetProperty("noi_dung").GetString());
        Assert.False(b.TryGetProperty("tieu_de", out _));
    }

    [Fact]
    public async Task ChayAsync_DocKetQuaVaoModelCuVaThongTinLuot()
    {
        var h = new HandlerTacVuGia().ThanhCong(
            """{"brief":"Quyết định phê duyệt kế hoạch","key_points":[{"heading":"Điều 1","content":"Phê duyệt"}]}""",
            creditDaTru: 7, soDu: 173, biCat: true);

        var kq = await TaoClient(h).ChayAsync<DocumentSummary>("tom-tat", new { noi_dung = "x" });

        Assert.Equal("Quyết định phê duyệt kế hoạch", kq.KetQua.Brief);
        Assert.Equal("Điều 1", kq.KetQua.KeyPoints[0].Heading);
        Assert.Equal(new ThongTinLuot(7, 173, "luot-1", true), kq.Luot);
        Assert.Null(kq.ChuoiTho);
    }

    [Fact]
    public async Task ChayAsync_KetQuaChuoi()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"I. KẾT QUẢ\\nII. PHƯƠNG HƯỚNG\"");

        var kq = await TaoClient(h).ChayAsync<string>("bao-cao-dinh-ky", new { loai_ky = "Tháng" });

        Assert.Equal("I. KẾT QUẢ\nII. PHƯƠNG HƯỚNG", kq.KetQua);
        Assert.Null(kq.ChuoiTho);
    }

    [Fact]
    public async Task ChayAsync_BiCatKetQuaLaChuoiTho_KhongNemLoiVaTraChuoiTho()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"{\\\"brief\\\":\\\"Quyết định phê duyệt, bị cắt giữa chừ\"", creditDaTru: 7, soDu: 173, biCat: true);

        var kq = await TaoClient(h).ChayAsync<DocumentSummary>("tom-tat", new { noi_dung = "x" });

        Assert.True(kq.Luot.BiCat);
        Assert.Equal(7, kq.Luot.CreditDaTru);
        Assert.Equal("{\"brief\":\"Quyết định phê duyệt, bị cắt giữa chừ", kq.ChuoiTho);
        Assert.Equal(JsonValueKind.String, kq.KetQuaTho.ValueKind);
        Assert.Null(kq.KetQua);
    }

    [Fact]
    public async Task ChayAsync_BiCatNhungKetQuaVanLaDoiTuong_DocBinhThuong()
    {
        var h = new HandlerTacVuGia().ThanhCong("""{"brief":"Gọn","key_points":[]}""", biCat: true);

        var kq = await TaoClient(h).ChayAsync<DocumentSummary>("tom-tat", new { noi_dung = "x" });

        Assert.Null(kq.ChuoiTho);
        Assert.Equal("Gọn", kq.KetQua.Brief);
        Assert.Equal(JsonValueKind.Object, kq.KetQuaTho.ValueKind);
    }

    [Fact]
    public async Task ChayAsync_KetQuaChuoiKhiKhongBiCatMaDoiTuongDuKien_BaoLoiMayChu()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"văn bản\"", biCat: false);

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<DocumentSummary>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(MaLoiTacVu.LoiMayChu, loi.Ma);
    }

    [Fact]
    public async Task ChayAsync_ThieuKetQua_BaoLoiMayChu()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK, """{"success":true,"data":{"credit_da_tru":1,"so_du":2}}""");

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(MaLoiTacVu.LoiMayChu, loi.Ma);
    }

    [Fact]
    public async Task UocTinhAsync_DocKhoangCreditVaSoDu()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK,
            """{"success":true,"message":"Thành công","data":{"credit_min":6,"credit_max":9,"so_du":180,"du_credit":true}}""");

        var u = await TaoClient(h).UocTinhAsync("tom-tat", new { noi_dung = "x" });

        Assert.Equal(new UocTinhKetQua(6, 9, 180, true), u);
        Assert.Equal("https://vbp.thu/api/v2/tac-vu/tom-tat/uoc-tinh", h.DaNhan[0].Url);
        Assert.Equal("desktop", h.DaNhan[0].Header["X-VBP-Nguon"]);
    }

    [Theory]
    [InlineData(402, "KHONG_DU_CREDIT", MaLoiTacVu.KhongDuCredit)]
    [InlineData(429, "QUA_NHIEU_LUOT", MaLoiTacVu.QuaNhieuLuot)]
    [InlineData(413, "QUA_DAI", MaLoiTacVu.QuaDai)]
    [InlineData(400, "DAU_VAO_SAI", MaLoiTacVu.DauVaoSai)]
    [InlineData(503, "AI_QUA_TAI", MaLoiTacVu.AiQuaTai)]
    [InlineData(502, "AI_LOI", MaLoiTacVu.AiLoi)]
    [InlineData(499, "NGAT_KET_NOI", MaLoiTacVu.NgatKetNoi)]
    [InlineData(401, null, MaLoiTacVu.ChuaDangNhap)]
    [InlineData(404, null, MaLoiTacVu.KhongCoTacVu)]
    [InlineData(500, null, MaLoiTacVu.LoiMayChu)]
    [InlineData(413, null, MaLoiTacVu.QuaDai)]
    [InlineData(504, null, MaLoiTacVu.HetThoiGian)]
    [InlineData(503, null, MaLoiTacVu.LoiMayChu)]
    public async Task ChayAsync_LoiMayChu_DoiRaDungMaVaKhongTuThuLai(int maHttp, string? code, MaLoiTacVu mong)
    {
        var than = code is null
            ? """{"success":false,"message":"Lỗi"}"""
            : $$"""{"success":false,"message":"Lỗi","code":"{{code}}"}""";
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)maHttp, than);

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(mong, loi.Ma);
        Assert.Equal(maHttp, loi.MaHttp);
        Assert.Single(h.DaNhan);
    }

    [Fact]
    public async Task AiQuaTai_DocRetryAfterVaGhiSoGiay()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.ServiceUnavailable,
            """{"success":false,"message":"Quá tải","code":"AI_QUA_TAI"}""",
            r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(20)));

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(TimeSpan.FromSeconds(20), loi.ThuLaiSau);
        Assert.Contains("20 giây", loi.Message);
        Assert.Contains("hoàn lại", loi.Message);
    }

    [Fact]
    public async Task QuaDai_KemChiTietMayChu()
    {
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)413,
            """{"success":false,"message":"Tổng dung lượng 12 MB vượt 10 MB của gói.","code":"QUA_DAI"}""");

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("trich-xuat", new { tep = Array.Empty<object>() }));

        Assert.Contains("12 MB vượt 10 MB", loi.Message);
        Assert.Equal("Tổng dung lượng 12 MB vượt 10 MB của gói.", loi.LoiMayChu);
    }

    [Fact]
    public async Task KhoaRong_BaoChuaDangNhapKhongGoiMayChu()
    {
        var h = new HandlerTacVuGia();

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h, khoa: "").UocTinhAsync("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(MaLoiTacVu.ChuaDangNhap, loi.Ma);
        Assert.Empty(h.DaNhan);
    }

    [Fact]
    public async Task MayChuTreo_HetHanChoBaoHetThoiGian()
    {
        var h = new HandlerTacVuGia { Treo = true };

        var loi = await Assert.ThrowsAsync<TacVuException>(
            () => TaoClient(h, han: TimeSpan.FromMilliseconds(200)).ChayAsync<string>("tom-tat", new { noi_dung = "x" })
                .WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.Equal(MaLoiTacVu.HetThoiGian, loi.Ma);
    }

    [Fact]
    public async Task MatMang_BaoMatKetNoi()
    {
        var h = new HandlerTacVuGia { NemLoi = new HttpRequestException(HttpRequestError.NameResolutionError, "No such host is known.") };

        var loi = await Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

        Assert.Equal(MaLoiTacVu.MatKetNoi, loi.Ma);
    }

    [Fact]
    public void HanChay_DuDaiChoTacVuDaiNhat()
    {
        Assert.True(TacVuClient.HanChayTheoTacVu("kiem-tra") >= TimeSpan.FromSeconds(150));
        Assert.True(TacVuClient.HanChayTheoTacVu("trich-xuat") >= TimeSpan.FromSeconds(300));
    }

    [Fact]
    public void HanChay_DungSoLieuHopDong()
    {
        Assert.Equal(TimeSpan.FromSeconds(180), TacVuClient.HanChayTheoTacVu("tom-tat"));
        Assert.Equal(TimeSpan.FromSeconds(180), TacVuClient.HanChayTheoTacVu("soan-thao"));
        Assert.Equal(TimeSpan.FromSeconds(300), TacVuClient.HanChayTheoTacVu("trich-xuat"));
        Assert.Equal(TimeSpan.FromSeconds(30), TacVuClient.HanUocTinh);
    }

    [Fact]
    public void LoiBaoKhach_CoDauVaKhongCoDaiTuNhanXung()
    {
        var cam = new Regex(@"\b(tôi|chúng tôi|ta|chúng ta|anh|chị|em|bạn|mình|dạ|vâng)\b|(^|\s)ạ([\s.,!?]|$)",
            RegexOptions.IgnoreCase);
        foreach (var ma in Enum.GetValues<MaLoiTacVu>())
        {
            foreach (int? maHttp in new int?[] { null, 500 })
            {
                var loi = TacVuClient.TaoLoi(ma, maHttp, "tom-tat", "chi tiết", TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(180));
                Assert.DoesNotMatch(cam, loi.Message);
                Assert.Matches("[ăâđêôơưàáạảãèéẹẻẽìíịỉĩòóọỏõùúụủũỳýỵỷỹ]", loi.Message.ToLowerInvariant());
            }
        }
    }
}
