using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIVanBan.Core.Services;
using AIVanBan.Tests.HoTro;

namespace AIVanBan.Tests;

/// <summary>Lỗi proxy, thân không phải JSON, hủy của bên gọi, hạn chờ thật, tiếng Việt trong thân gửi.</summary>
public class TacVuClientDoTinCayTests
{
    private const string CauCredit = "Credit giữ tạm (nếu có) được hoàn tự động trong vòng 15 phút nếu lượt không hoàn tất; xem số dư ở trang Tài khoản.";
    private const string TrangHtml = "<html><head><title>Error</title></head><body><h1>Gateway error</h1></body></html>";

    private static TacVuClient TaoClient(HandlerTacVuGia h, TimeSpan? han = null, TimeProvider? dongHo = null) =>
        new(h, "https://vbp.thu", "khoa-thu", "1.0.20", han, dongHo);

    private static Task<TacVuException> ChayLoi(HandlerTacVuGia h) =>
        Assert.ThrowsAsync<TacVuException>(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }));

    // --- Thân không phải JSON ---

    [Theory]
    [InlineData(524)]
    [InlineData(522)]
    [InlineData(523)]
    [InlineData(504)]
    public async Task ProxyCatQuaHan_TrangHtml_BaoHetThoiGianKemCauCredit(int maHttp)
    {
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)maHttp, TrangHtml);

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.HetThoiGian, loi.Ma);
        Assert.Equal(maHttp, loi.MaHttp);
        Assert.Contains(CauCredit, loi.Message);
        Assert.Contains($"mã {maHttp}", loi.Message);
    }

    [Fact]
    public async Task Html502_BaoLoiMayChuKemCauCredit()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.BadGateway, TrangHtml);

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.LoiMayChu, loi.Ma);
        Assert.Contains(CauCredit, loi.Message);
    }

    [Fact]
    public async Task ThanRong_500_BaoLoiMayChuKemCauCredit()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.InternalServerError, "");

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.LoiMayChu, loi.Ma);
        Assert.Contains(CauCredit, loi.Message);
    }

    [Fact]
    public async Task Http200ThanHtml_BaoLoiMayChu()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK, TrangHtml);

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.LoiMayChu, loi.Ma);
        Assert.Equal(200, loi.MaHttp);
    }

    [Fact]
    public async Task Http200ThanRong_BaoLoiMayChu()
    {
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK, "");

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.LoiMayChu, loi.Ma);
    }

    [Fact]
    public async Task HetHanChoPhiaMay_CungCoCauCredit()
    {
        var h = new HandlerTacVuGia { Treo = true };

        var loi = await Assert.ThrowsAsync<TacVuException>(
            () => TaoClient(h, TimeSpan.FromMilliseconds(100)).ChayAsync<string>("tom-tat", new { noi_dung = "x" })
                .WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.Equal(MaLoiTacVu.HetThoiGian, loi.Ma);
        Assert.Contains(CauCredit, loi.Message);
    }

    // --- Mất kết nối: trước khi gửi và sau khi gửi ---

    private sealed class ThanDocLoi : HttpContent
    {
        public bool DaGiaiPhong { get; private set; }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            throw new IOException("Mất kết nối giữa chừng.");

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            DaGiaiPhong = true;
            base.Dispose(disposing);
        }
    }

    [Fact]
    public async Task MatKetNoiKhiDocThan_YeuCauDaGui_CoCauCreditVaGiaiPhongPhanHoi()
    {
        var than = new ThanDocLoi();
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK, "{}", r => r.Content = than);

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.MatKetNoi, loi.Ma);
        Assert.Contains(CauCredit, loi.Message);
        Assert.True(than.DaGiaiPhong);
    }

    [Fact]
    public async Task MatKetNoiTruocKhiGui_KhongCoCauCredit()
    {
        var h = new HandlerTacVuGia { NemLoi = new HttpRequestException("No such host is known.") };

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.MatKetNoi, loi.Ma);
        Assert.DoesNotContain("Credit giữ tạm", loi.Message);
    }

    // --- Hủy của bên gọi ---

    [Fact]
    public async Task TokenDaHuy_NemOperationCanceled_KhongPhaiHetThoiGian()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"x\"");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var ex = await Record.ExceptionAsync(() => TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }, cts.Token));

        Assert.IsAssignableFrom<OperationCanceledException>(ex);
        Assert.IsNotType<TacVuException>(ex);
    }

    [Fact]
    public async Task HuyKhiDangCho_NemOperationCanceled_KhongPhaiHetThoiGian()
    {
        var h = new HandlerTacVuGia { Treo = true };
        using var cts = new CancellationTokenSource();

        var tac = TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "x" }, cts.Token);
        cts.CancelAfter(100);
        var ex = await Record.ExceptionAsync(() => tac.WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.IsAssignableFrom<OperationCanceledException>(ex);
        Assert.IsNotType<TacVuException>(ex);
    }

    // --- Hạn chờ thật sự được áp dụng ---

    private sealed class DongHoGhiHan : TimeProvider
    {
        public List<TimeSpan> Han { get; } = new();

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            Han.Add(dueTime);
            return new HenGioKhongChay();
        }

        private sealed class HenGioKhongChay : ITimer
        {
            public bool Change(TimeSpan dueTime, TimeSpan period) => true;
            public void Dispose() { }
            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    [Theory]
    [InlineData("trich-xuat", 300)]
    [InlineData("tom-tat", 180)]
    [InlineData("kiem-tra", 180)]
    [InlineData("soan-thao", 180)]
    public async Task ChayAsync_AppDungHanChoTheoTacVu(string ten, int giayMong)
    {
        var dongHo = new DongHoGhiHan();
        var h = new HandlerTacVuGia().ThanhCong("\"x\"");

        await TaoClient(h, dongHo: dongHo).ChayAsync<string>(ten, new { noi_dung = "x" });

        Assert.Equal(TimeSpan.FromSeconds(giayMong), Assert.Single(dongHo.Han));
    }

    [Fact]
    public async Task UocTinhAsync_AppDungHan30Giay()
    {
        var dongHo = new DongHoGhiHan();
        var h = new HandlerTacVuGia().TraVe(HttpStatusCode.OK,
            """{"success":true,"data":{"credit_min":1,"credit_max":2,"so_du":3,"du_credit":true}}""");

        await TaoClient(h, dongHo: dongHo).UocTinhAsync("trich-xuat", new { tep = Array.Empty<object>() });

        Assert.Equal(TimeSpan.FromSeconds(30), Assert.Single(dongHo.Han));
    }

    // --- Giữ chi tiết của máy chủ ---

    [Fact]
    public async Task KhongDuCredit_KemLoiMayChuCoSoThat()
    {
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)402,
            """{"success":false,"message":"Cần tối đa 9 credit, số dư còn 4.","code":"KHONG_DU_CREDIT"}""");

        var loi = await ChayLoi(h);

        Assert.Equal(MaLoiTacVu.KhongDuCredit, loi.Ma);
        Assert.Contains("Cần tối đa 9 credit, số dư còn 4.", loi.Message);
        Assert.Equal("Cần tối đa 9 credit, số dư còn 4.", loi.LoiMayChu);
    }

    [Fact]
    public async Task QuaNhieuLuot_CoLoiMayChu_BoSo3CungKem()
    {
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)429,
            """{"success":false,"message":"Đang chạy 2 lượt, hạn mức gói là 2.","code":"QUA_NHIEU_LUOT"}""");

        var loi = await ChayLoi(h);

        Assert.Contains("Đang chạy 2 lượt, hạn mức gói là 2.", loi.Message);
        Assert.DoesNotContain("3 tác vụ", loi.Message);
    }

    [Fact]
    public async Task QuaNhieuLuot_KhongCoLoiMayChu_GiuCau3TacVu()
    {
        var h = new HandlerTacVuGia().TraVe((HttpStatusCode)429, """{"success":false,"code":"QUA_NHIEU_LUOT"}""");

        var loi = await ChayLoi(h);

        Assert.Contains("3 tác vụ", loi.Message);
        Assert.DoesNotContain("Chi tiết", loi.Message);
    }

    // --- Gửi UTF-8 nguyên chữ ---

    [Fact]
    public async Task ThanGui_TiengVietLaUtf8NguyenChuKhongThoatUnicode()
    {
        var h = new HandlerTacVuGia().ThanhCong("\"x\"");

        await TaoClient(h).ChayAsync<string>("tom-tat", new { noi_dung = "Đồng Nai: Điều 1 — phê duyệt" });

        var y = Assert.Single(h.DaNhan);
        Assert.NotNull(y.Bytes);
        // "Đ" = C4 90 trong UTF-8.
        Assert.Contains((byte)0xC4, y.Bytes!);
        Assert.Contains((byte)0x90, y.Bytes!);
        Assert.DoesNotContain("\\u", y.Body);
        using var doc = JsonDocument.Parse(y.Bytes!);
        Assert.Equal("Đồng Nai: Điều 1 — phê duyệt", doc.RootElement.GetProperty("noi_dung").GetString());
    }

    // --- Tiện ích BiCatChuoiTho ---

    [Fact]
    public async Task BiCatChuoiTho_PhanAnhChuoiTho()
    {
        var bi = await TaoClient(new HandlerTacVuGia().ThanhCong("\"{\\\"brief\\\":\\\"cắt\"", biCat: true))
            .ChayAsync<AIVanBan.Core.Models.DocumentSummary>("tom-tat", new { noi_dung = "x" });
        var thuong = await TaoClient(new HandlerTacVuGia().ThanhCong("\"x\"", biCat: true))
            .ChayAsync<string>("bao-cao-dinh-ky", new { loai_ky = "Tháng" });

        Assert.True(bi.BiCatChuoiTho);
        Assert.False(thuong.BiCatChuoiTho);
    }

    // --- Lời báo khách ---

    [Fact]
    public void LoiBaoKhach_MoiNhanhKhongCoDaiTuNhanXung()
    {
        var cam = new Regex(@"\b(tôi|chúng tôi|ta|chúng ta|anh|chị|em|bạn|mình|dạ|vâng)\b|(^|\s)ạ([\s.,!?]|$)", RegexOptions.IgnoreCase);
        foreach (var ma in Enum.GetValues<MaLoiTacVu>())
            foreach (int? maHttp in new int?[] { null, 200, 500, 504, 524 })
                foreach (var daGui in new[] { false, true })
                    foreach (var chiTiet in new string?[] { null, "Cần tối đa 9 credit, số dư còn 4." })
                    {
                        var loi = TacVuClient.TaoLoi(ma, maHttp, "tom-tat", chiTiet, TimeSpan.FromSeconds(20),
                            TimeSpan.FromSeconds(180), daGuiYeuCau: daGui);
                        Assert.DoesNotMatch(cam, loi.Message);
                    }
    }
}
