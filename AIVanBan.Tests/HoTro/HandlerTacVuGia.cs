using System.Net;
using System.Net.Http;
using System.Text;

namespace AIVanBan.Tests.HoTro;

/// <summary>
/// HttpMessageHandler giả cho client tác vụ v2: trả lần lượt các phản hồi đã định, ghi lại từng yêu cầu
/// (phương thức, URL, header, thân). Không có phản hồi nào trong hàng đợi thì bài kiểm hỏng rõ ràng.
/// </summary>
internal sealed class HandlerTacVuGia : HttpMessageHandler
{
    public sealed record YeuCau(HttpMethod Method, string Url, Dictionary<string, string> Header, string Body);

    private readonly Queue<Func<HttpResponseMessage>> _phanHoi = new();

    public List<YeuCau> DaNhan { get; } = new();

    /// <summary>true: không bao giờ trả lời (để thử hết hạn chờ).</summary>
    public bool Treo { get; init; }

    /// <summary>Khác null: ném lỗi này thay vì trả lời (để thử mất kết nối).</summary>
    public Exception? NemLoi { get; init; }

    public HandlerTacVuGia TraVe(HttpStatusCode ma, string json, Action<HttpResponseMessage>? sua = null)
    {
        _phanHoi.Enqueue(() =>
        {
            var r = new HttpResponseMessage(ma) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            sua?.Invoke(r);
            return r;
        });
        return this;
    }

    public HandlerTacVuGia ThanhCong(string ketQuaJson, int creditDaTru = 7, int soDu = 173, bool biCat = false) =>
        TraVe(HttpStatusCode.OK,
            $$$"""{"success":true,"message":"Thành công","data":{"ket_qua":{{{ketQuaJson}}},"credit_da_tru":{{{creditDaTru}}},"so_du":{{{soDu}}},"ma_luot":"luot-1","bi_cat":{{{(biCat ? "true" : "false")}}}}}""");

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var header = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value), StringComparer.OrdinalIgnoreCase);
        if (request.Content != null)
            foreach (var h in request.Content.Headers)
                header[h.Key] = string.Join(",", h.Value);
        var body = request.Content == null ? "" : await request.Content.ReadAsStringAsync(ct);
        DaNhan.Add(new YeuCau(request.Method, request.RequestUri!.ToString(), header, body));

        if (NemLoi != null) throw NemLoi;
        if (Treo) await Task.Delay(Timeout.Infinite, ct);
        return _phanHoi.Dequeue()();
    }
}
