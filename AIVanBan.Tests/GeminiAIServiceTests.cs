using System.Net;
using System.Net.Http;
using System.Text;
using AIVanBan.Core.Services;

namespace AIVanBan.Tests;

public class GeminiAIServiceTests
{
    private const string GeminiOk =
        """{"candidates":[{"content":{"parts":[{"text":"kết quả"}]},"finishReason":"STOP"}]}""";
    private const string VanBanPlusOk =
        """{"success":true,"data":{"content":"kết quả"}}""";

    /// <summary>Trả lần lượt các phản hồi đã định; ghi lại từng request đã nhận.</summary>
    private sealed class HandlerGia : HttpMessageHandler
    {
        private readonly Queue<(HttpStatusCode Ma, string Noi)> _phanHoi;
        private readonly bool _treo;
        public List<string> Urls { get; } = new();

        public HandlerGia(params (HttpStatusCode, string)[] phanHoi)
        {
            _phanHoi = new Queue<(HttpStatusCode, string)>(phanHoi);
        }

        private HandlerGia(bool treo)
        {
            _phanHoi = new Queue<(HttpStatusCode, string)>();
            _treo = treo;
        }

        public static HandlerGia LuonTreo() => new(treo: true);

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Urls.Add(request.RequestUri!.ToString());
            if (_treo)
                await Task.Delay(Timeout.Infinite, cancellationToken);

            var (ma, noi) = _phanHoi.Dequeue();
            return new HttpResponseMessage(ma)
            {
                Content = new StringContent(noi, Encoding.UTF8, "application/json")
            };
        }
    }

    private static GeminiAIService TaoService(HandlerGia handler, bool quaVanBanPlus = false, int hanChoMs = 60000) =>
        new(handler, quaVanBanPlus, new[] { 0, 0, 0 }, TimeSpan.FromMilliseconds(hanChoMs));

    private static string TepTam()
    {
        var duongDan = Path.Combine(Path.GetTempPath(), $"thu-ai-{Guid.NewGuid():N}.png");
        File.WriteAllBytes(duongDan, new byte[] { 1, 2, 3 });
        return duongDan;
    }

    private static async Task<string> DocHetStream(GeminiAIService service)
    {
        var ketQua = new StringBuilder();
        await foreach (var doan in service.GenerateContentStreamAsync("câu hỏi"))
            ketQua.Append(doan);
        return ketQua.ToString();
    }

    [Fact]
    public async Task GoiTrucTiep_DungModelDangOnDinh()
    {
        var handler = new HandlerGia((HttpStatusCode.OK, GeminiOk));

        await TaoService(handler).GenerateContentAsync("câu hỏi");

        Assert.Contains("/gemini-3.5-flash:generateContent", handler.Urls[0]);
    }

    [Fact]
    public async Task GenerateContent_ThuLaiKhiGap503()
    {
        var handler = new HandlerGia(
            (HttpStatusCode.ServiceUnavailable, "{}"),
            (HttpStatusCode.ServiceUnavailable, "{}"),
            (HttpStatusCode.OK, GeminiOk));

        var ketQua = await TaoService(handler).GenerateContentAsync("câu hỏi");

        Assert.Equal("kết quả", ketQua);
        Assert.Equal(3, handler.Urls.Count);
    }

    [Fact]
    public async Task Streaming_ThuLaiKhiGap503()
    {
        var handler = new HandlerGia(
            (HttpStatusCode.ServiceUnavailable, "{}"),
            (HttpStatusCode.OK, $"data: {GeminiOk}\n\n"));

        var ketQua = await DocHetStream(TaoService(handler));

        Assert.Equal("kết quả", ketQua);
        Assert.Equal(2, handler.Urls.Count);
    }

    [Fact]
    public async Task GenerateContent_VanQuaTaiSauKhiThuLai_BaoQuaTaiRoRang()
    {
        var handler = new HandlerGia(
            (HttpStatusCode.ServiceUnavailable, "{}"),
            (HttpStatusCode.ServiceUnavailable, "{}"),
            (HttpStatusCode.ServiceUnavailable, "{}"));

        var loi = await Assert.ThrowsAsync<Exception>(() => TaoService(handler).GenerateContentAsync("câu hỏi"));

        Assert.Contains("quá tải", loi.Message);
        Assert.Equal(3, handler.Urls.Count);
    }

    [Fact]
    public async Task GenerateContent_MayChuTreo_DungDungHanVaBaoQuaLau()
    {
        var service = TaoService(HandlerGia.LuonTreo(), hanChoMs: 200);

        var loi = await Assert.ThrowsAsync<Exception>(
            () => service.GenerateContentAsync("câu hỏi").WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.Contains("quá lâu", loi.Message);
    }

    [Fact]
    public async Task Streaming_MayChuTreo_DungDungHanVaBaoQuaLau()
    {
        var service = TaoService(HandlerGia.LuonTreo(), hanChoMs: 200);

        var loi = await Assert.ThrowsAsync<TimeoutException>(
            () => DocHetStream(service).WaitAsync(TimeSpan.FromSeconds(10)));

        Assert.Contains("quá lâu", loi.Message);
    }

    [Fact]
    public async Task VanBanPlus_KhoaBiTuChoi_BaoDangNhapLaiVaKhongThuLai()
    {
        var handler = new HandlerGia((HttpStatusCode.Unauthorized, """{"success":false}"""));

        var loi = await Assert.ThrowsAsync<Exception>(
            () => TaoService(handler, quaVanBanPlus: true).GenerateContentAsync("câu hỏi"));

        Assert.Contains("đăng nhập lại", loi.Message);
        Assert.Single(handler.Urls);
    }

    [Fact]
    public async Task VanBanPlus_TrichXuat_KhoaBiTuChoi_BaoDangNhapLai()
    {
        var handler = new HandlerGia((HttpStatusCode.Unauthorized, """{"success":false}"""));
        var tep = TepTam();
        try
        {
            var loi = await Assert.ThrowsAsync<Exception>(
                () => TaoService(handler, quaVanBanPlus: true).ExtractDocumentFromFileAsync(tep));

            Assert.Contains("đăng nhập lại", loi.Message);
            // Hộp thoại quét tài liệu nhận diện lỗi xác thực qua mã "401" trong thông điệp.
            Assert.Contains("401", loi.Message);
        }
        finally
        {
            File.Delete(tep);
        }
    }

    [Fact]
    public async Task VanBanPlus_DocChu_KhoaBiTuChoi_BaoDangNhapLai()
    {
        var handler = new HandlerGia((HttpStatusCode.Unauthorized, """{"success":false}"""));
        var tep = TepTam();
        try
        {
            var loi = await Assert.ThrowsAsync<HttpRequestException>(
                () => TaoService(handler, quaVanBanPlus: true).ReadTextFromFileAsync(tep));

            Assert.Contains("đăng nhập lại", loi.Message);
        }
        finally
        {
            File.Delete(tep);
        }
    }

    [Fact]
    public async Task VanBanPlus_KhoaHopLe_TraNoiDung()
    {
        var handler = new HandlerGia((HttpStatusCode.OK, VanBanPlusOk));

        var ketQua = await TaoService(handler, quaVanBanPlus: true).GenerateContentAsync("câu hỏi");

        Assert.Equal("kết quả", ketQua);
        Assert.EndsWith("/api/ai/generate", handler.Urls[0]);
    }
}
