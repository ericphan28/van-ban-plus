using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIVanBan.Core.Services;

/// <summary>Kết quả /uoc-tinh: khoảng credit dự kiến và số dư hiện có.</summary>
public sealed record UocTinhKetQua(int CreditMin, int CreditMax, int SoDu, bool DuCredit);

/// <summary>Thông tin một lượt chạy xong.</summary>
public sealed record ThongTinLuot(int CreditDaTru, int SoDu, string MaLuot, bool BiCat);

/// <summary>
/// Kết quả một lượt chạy. <see cref="KetQua"/> là data.ket_qua đã đọc vào T.
/// Khi kết quả bị cắt (<see cref="ThongTinLuot.BiCat"/>) và tác vụ JSON trả CHUỖI THÔ thay vì đối tượng
/// (hợp đồng), <see cref="KetQua"/> là <c>default</c> (null với kiểu tham chiếu) và <see cref="ChuoiTho"/>
/// giữ chuỗi đó; bên gọi phải kiểm <see cref="ChuoiTho"/> trước khi dùng <see cref="KetQua"/>.
/// Credit vẫn bị trừ theo <see cref="ThongTinLuot.CreditDaTru"/>.
/// </summary>
/// <param name="KetQuaTho">data.ket_qua nguyên dạng JSON (chưa đọc vào T).</param>
/// <param name="ChuoiTho">Khác null chỉ khi T không phải string mà máy chủ trả chuỗi thô (kết quả bị cắt).</param>
public sealed record TacVuKetQua<T>(T KetQua, ThongTinLuot Luot, JsonElement KetQuaTho = default, string? ChuoiTho = null)
{
    /// <summary>true khi kết quả bị cắt và máy chủ trả chuỗi thô: <see cref="KetQua"/> không dùng được, xem <see cref="ChuoiTho"/>.</summary>
    public bool BiCatChuoiTho => ChuoiTho is not null;
}

/// <summary>
/// Gọi API tác vụ v2 của VanBanPlus: POST /api/v2/tac-vu/&lt;ten&gt; và /uoc-tinh.
/// Bản cài máy chỉ gửi dữ liệu nghiệp vụ; máy chủ dựng prompt, gọi AI bằng khóa hệ thống và tính credit.
/// Không tự thử lại: mỗi lần chạy là một lượt giữ credit, để người dùng quyết định.
/// </summary>
public sealed class TacVuClient
{
    /// <summary>Hạn chờ một lượt chạy. Lượt dài nhất đo được 47 giây (spec §7), để rộng gần 4 lần.</summary>
    public static readonly TimeSpan HanChay = TimeSpan.FromSeconds(180);

    /// <summary>trich-xuat gửi tới 20 tệp, phần tải lên lâu hơn.</summary>
    public static readonly TimeSpan HanTrichXuat = TimeSpan.FromSeconds(300);

    private static readonly HttpClient HttpDungChung = new() { Timeout = Timeout.InfiniteTimeSpan };

    private static readonly JsonSerializerOptions TuyChonGui = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // Gửi tiếng Việt là UTF-8 nguyên chữ, không phải \uXXXX.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    internal static readonly JsonSerializerOptions TuyChonDoc = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly HttpClient _http;
    private readonly Func<(string DiaChi, string Khoa)> _ketNoi;
    private readonly string _phienBan;
    private readonly Func<string, TimeSpan> _hanChay;
    private readonly TimeProvider _dongHo;

    /// <summary>Đọc địa chỉ máy chủ và khóa từ settings.json ở MỖI lần gọi (đăng nhập lại có hiệu lực ngay).</summary>
    public TacVuClient() : this(HttpDungChung, DocKetNoiTuThietLap, PhienBanUngDung(), HanChayTheoTacVu) { }

    /// <summary>Cho bộ kiểm: handler giả, địa chỉ và khóa cố định, không đọc settings.json.</summary>
    internal TacVuClient(HttpMessageHandler handler, string diaChi, string khoa, string phienBan, TimeSpan? hanChay = null,
        TimeProvider? dongHo = null)
        : this(new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
               () => (diaChi.TrimEnd('/'), khoa),
               phienBan,
               hanChay is { } h ? _ => h : HanChayTheoTacVu,
               dongHo)
    {
    }

    private TacVuClient(HttpClient http, Func<(string DiaChi, string Khoa)> ketNoi, string phienBan, Func<string, TimeSpan> hanChay,
        TimeProvider? dongHo = null)
    {
        _dongHo = dongHo ?? TimeProvider.System;
        _http = http;
        _ketNoi = ketNoi;
        _phienBan = phienBan;
        _hanChay = hanChay;
    }

    private static (string DiaChi, string Khoa) DocKetNoiTuThietLap()
    {
        var s = AppSettingsService.Load();
        return ((s.VanBanPlusApiUrl ?? "").Trim().TrimEnd('/'), (s.VanBanPlusApiKey ?? "").Trim());
    }

    /// <summary>Phiên bản ứng dụng gửi trong X-VBP-Phien-Ban, vd "1.0.20" (từ &lt;Version&gt; của AIVanBan.Desktop.csproj).</summary>
    internal static string PhienBanUngDung() =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";

    internal static TimeSpan HanChayTheoTacVu(string ten) => ten == "trich-xuat" ? HanTrichXuat : HanChay;

    /// <summary>
    /// POST /api/v2/tac-vu/&lt;ten&gt;/uoc-tinh — không gọi AI, không giữ credit. Tải lên đúng thân của lượt chạy
    /// (trich-xuat tới 20 MB) nên dùng cùng hạn chờ với lượt chạy của tác vụ đó.
    /// </summary>
    public async Task<UocTinhKetQua> UocTinhAsync(string ten, object input, CancellationToken ct = default)
    {
        var data = await GoiAsync(ten, $"/api/v2/tac-vu/{ten}/uoc-tinh", input, _hanChay(ten), ct, uocTinh: true);
        return new UocTinhKetQua(
            DocSo(data, "credit_min"),
            DocSo(data, "credit_max"),
            DocSo(data, "so_du"),
            data.TryGetProperty("du_credit", out var du) && du.ValueKind == JsonValueKind.True);
    }

    /// <summary>
    /// POST /api/v2/tac-vu/&lt;ten&gt; — đọc data.ket_qua vào T (model C# hiện có, hoặc string).
    /// Kết quả bị cắt mà ket_qua là chuỗi thô (T không phải string): không ném lỗi, trả <see cref="TacVuKetQua{T}.ChuoiTho"/>.
    /// </summary>
    public async Task<TacVuKetQua<T>> ChayAsync<T>(string ten, object input, CancellationToken ct = default)
    {
        var han = _hanChay(ten);
        var data = await GoiAsync(ten, $"/api/v2/tac-vu/{ten}", input, han, ct);

        if (!data.TryGetProperty("ket_qua", out var kq) || kq.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            throw TaoLoi(MaLoiTacVu.LoiMayChu, 200, ten, "Thiếu ket_qua.", null, han);

        var luot = new ThongTinLuot(
            DocSo(data, "credit_da_tru"),
            DocSo(data, "so_du"),
            data.TryGetProperty("ma_luot", out var ml) && ml.ValueKind == JsonValueKind.String ? ml.GetString()! : "",
            data.TryGetProperty("bi_cat", out var bc) && bc.ValueKind == JsonValueKind.True);

        // Hợp đồng: bi_cat + tác vụ JSON không đọc được → ket_qua là chuỗi thô (vẫn trừ credit).
        if (luot.BiCat && typeof(T) != typeof(string) && kq.ValueKind == JsonValueKind.String)
            return new TacVuKetQua<T>(default!, luot, kq, kq.GetString() ?? "");

        T? ketQua;
        try
        {
            ketQua = kq.Deserialize<T>(TuyChonDoc);
        }
        catch (JsonException ex)
        {
            throw TaoLoi(MaLoiTacVu.LoiMayChu, 200, ten, ex.Message, null, han, ex);
        }
        if (ketQua is null)
            throw TaoLoi(MaLoiTacVu.LoiMayChu, 200, ten, "ket_qua rỗng.", null, han);

        return new TacVuKetQua<T>(ketQua, luot, kq);
    }

    private async Task<JsonElement> GoiAsync(string ten, string duongDan, object input, TimeSpan han, CancellationToken ct,
        bool uocTinh = false)
    {
        var (diaChi, khoa) = _ketNoi();
        if (string.IsNullOrWhiteSpace(khoa))
            throw TaoLoi(MaLoiTacVu.ChuaDangNhap, null, ten, null, null, han);

        // Hạn chờ đo bằng _dongHo (bộ kiểm thay được); ct của bên gọi hủy thì ném OperationCanceledException, không phải HetThoiGian.
        using var hetHan = new CancellationTokenSource(han, _dongHo);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, hetHan.Token);

        using var yeuCau = new HttpRequestMessage(HttpMethod.Post, diaChi + duongDan)
        {
            Content = new StringContent(JsonSerializer.Serialize(input, TuyChonGui), Encoding.UTF8, "application/json"),
        };
        yeuCau.Headers.TryAddWithoutValidation("X-API-Key", khoa);
        yeuCau.Headers.TryAddWithoutValidation("X-VBP-Nguon", "desktop");
        yeuCau.Headers.TryAddWithoutValidation("X-VBP-Phien-Ban", _phienBan);

        HttpResponseMessage? phanHoi = null;
        try
        {
            // Hai pha tách riêng: lỗi ở pha gửi (chưa tới máy chủ) khác lỗi ở pha đọc thân (yêu cầu đã gửi, có thể đã giữ credit).
            try
            {
                phanHoi = await _http.SendAsync(yeuCau, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw TaoLoi(MaLoiTacVu.HetThoiGian, null, ten, null, null, han, uocTinh: uocTinh);
            }
            catch (HttpRequestException ex)
            {
                // Chỉ bốn loại lỗi này chắc chắn yêu cầu chưa rời máy; loại khác (giao thức, kết nối đứt giữa chừng, không rõ)
                // có thể yêu cầu đã tới máy chủ và đang giữ credit.
                throw TaoLoi(MaLoiTacVu.MatKetNoi, null, ten, ex.Message, null, han, ex,
                    daGuiYeuCau: !YeuCauChuaRoiMay(ex), uocTinh: uocTinh);
            }

            string than;
            try
            {
                than = await phanHoi.Content.ReadAsStringAsync(cts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                throw TaoLoi(MaLoiTacVu.HetThoiGian, null, ten, null, null, han, uocTinh: uocTinh);
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException)
            {
                throw TaoLoi(MaLoiTacVu.MatKetNoi, null, ten, ex.Message, null, han, ex, daGuiYeuCau: true, uocTinh: uocTinh);
            }

            JsonElement? goc = null;
            try
            {
                using var doc = JsonDocument.Parse(than);
                goc = doc.RootElement.Clone();
            }
            catch (JsonException)
            {
                // Thân không phải JSON (vd trang HTML lỗi của proxy): phân loại theo mã HTTP.
            }

            if (phanHoi.IsSuccessStatusCode
                && goc is { ValueKind: JsonValueKind.Object } g
                && g.TryGetProperty("success", out var s) && s.ValueKind == JsonValueKind.True
                && g.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Object)
            {
                return d;
            }

            string? code = null, message = null;
            if (goc is { ValueKind: JsonValueKind.Object } e)
            {
                if (e.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String) code = c.GetString();
                if (e.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String) message = m.GetString();
            }

            TimeSpan? thuLaiSau = phanHoi.Headers.RetryAfter?.Delta;
            if (thuLaiSau is null && phanHoi.Headers.RetryAfter?.Date is { } luc)
                thuLaiSau = luc - DateTimeOffset.UtcNow;

            var maHttp = (int)phanHoi.StatusCode;
            throw TaoLoi(PhanLoai(maHttp, code), maHttp, ten, message, thuLaiSau, han, uocTinh: uocTinh);
        }
        finally
        {
            phanHoi?.Dispose();
        }
    }

    /// <summary>Lỗi ở pha gửi mà yêu cầu chắc chắn chưa tới máy chủ: không phân giải được tên, không nối được, TLS hỏng, proxy từ chối.</summary>
    internal static bool YeuCauChuaRoiMay(HttpRequestException ex) =>
        ex.HttpRequestError is HttpRequestError.NameResolutionError or HttpRequestError.ConnectionError
            or HttpRequestError.SecureConnectionError or HttpRequestError.ProxyTunnelError;

    private static int DocSo(JsonElement data, string ten)
    {
        if (!data.TryGetProperty(ten, out var v)) return 0;
        if (v.ValueKind == JsonValueKind.Number)
            return v.TryGetInt32(out var i) ? i : (int)Math.Ceiling(v.GetDouble());
        if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var j)) return j;
        return 0;
    }

    /// <summary>Ưu tiên trường code của hợp đồng; không có code thì xét mã HTTP.</summary>
    internal static MaLoiTacVu PhanLoai(int maHttp, string? code) => code switch
    {
        "KHONG_DU_CREDIT" => MaLoiTacVu.KhongDuCredit,
        "QUA_NHIEU_LUOT" => MaLoiTacVu.QuaNhieuLuot,
        "QUA_DAI" => MaLoiTacVu.QuaDai,
        "DAU_VAO_SAI" => MaLoiTacVu.DauVaoSai,
        "AI_QUA_TAI" => MaLoiTacVu.AiQuaTai,
        "AI_LOI" => MaLoiTacVu.AiLoi,
        "NGAT_KET_NOI" => MaLoiTacVu.NgatKetNoi,
        _ => maHttp switch
        {
            400 => MaLoiTacVu.DauVaoSai,
            401 => MaLoiTacVu.ChuaDangNhap,
            402 => MaLoiTacVu.KhongDuCredit,
            404 => MaLoiTacVu.KhongCoTacVu,
            413 => MaLoiTacVu.QuaDai,
            429 => MaLoiTacVu.QuaNhieuLuot,
            499 => MaLoiTacVu.NgatKetNoi,
            // 504 và 522/523/524 (Cloudflare cắt ở 100 giây, thân thường là trang HTML không có code).
            504 or 522 or 523 or 524 => MaLoiTacVu.HetThoiGian,
            _ => MaLoiTacVu.LoiMayChu,
        },
    };

    /// <summary>Câu báo tình trạng credit khi chưa rõ lượt đã hoàn tất hay chưa.</summary>
    internal const string CauTrangThaiCredit =
        "Credit giữ tạm (nếu có) được hoàn tự động trong vòng 15 phút nếu lượt không hoàn tất; xem số dư ở trang Tài khoản.";

    /// <summary>
    /// Dựng lỗi kèm lời hiện cho khách (tiếng Việt có dấu, không đại từ nhân xưng).
    /// <paramref name="daGuiYeuCau"/>: yêu cầu có thể đã tới máy chủ (mất kết nối lúc đọc thân, hoặc lỗi gửi không chắc chưa rời máy).
    /// <paramref name="uocTinh"/>: lời cho /uoc-tinh, không bao giờ kèm câu về credit vì ước tính không giữ credit.
    /// </summary>
    internal static TacVuException TaoLoi(MaLoiTacVu ma, int? maHttp, string ten, string? chiTiet,
        TimeSpan? thuLaiSau, TimeSpan han, Exception? goc = null, bool daGuiYeuCau = false, bool uocTinh = false)
    {
        var cau = uocTinh ? "" : " " + CauTrangThaiCredit;
        static string KemChiTiet(string? s) => string.IsNullOrWhiteSpace(s) ? "" : $"\nChi tiết: {s}";

        var thongDiep = ma switch
        {
            MaLoiTacVu.KhongDuCredit =>
                "Số dư credit không đủ cho tác vụ này. Credit chưa bị trừ. Có thể mua thêm credit trên trang Bảng giá của VanBanPlus."
                + KemChiTiet(chiTiet),
            MaLoiTacVu.QuaNhieuLuot =>
                (string.IsNullOrWhiteSpace(chiTiet)
                    ? "Tài khoản đang có 3 tác vụ AI chạy cùng lúc."
                    : "Tài khoản đang có nhiều tác vụ AI chạy cùng lúc.")
                + " Chờ một tác vụ xong rồi thử lại. Credit chưa bị trừ." + KemChiTiet(chiTiet),
            MaLoiTacVu.QuaDai =>
                "Nội dung hoặc tệp gửi lên vượt giới hạn của gói hiện tại. Cần rút ngắn nội dung, giảm dung lượng tệp, bớt số trang hoặc nâng cấp gói. Credit chưa bị trừ."
                + KemChiTiet(chiTiet),
            MaLoiTacVu.DauVaoSai =>
                "Dữ liệu gửi lên chưa hợp lệ. Credit chưa bị trừ." + KemChiTiet(chiTiet),
            MaLoiTacVu.AiQuaTai =>
                "Máy chủ AI đang quá tải. Credit đã được hoàn lại. "
                + (thuLaiSau is { } t && t > TimeSpan.Zero
                    ? $"Thử lại sau khoảng {Math.Ceiling(t.TotalSeconds):0} giây."
                    : "Thử lại sau ít phút."),
            MaLoiTacVu.AiLoi =>
                "AI gặp lỗi khi xử lý. Credit đã được hoàn lại. Thử lại sau ít phút.",
            MaLoiTacVu.NgatKetNoi =>
                "Kết nối bị ngắt trước khi nhận được kết quả. Credit đã được hoàn lại. Thử lại.",
            MaLoiTacVu.ChuaDangNhap => maHttp is null
                ? "Máy này chưa đăng nhập tài khoản VanBanPlus. Cần bấm «Đăng nhập» trước khi dùng tính năng AI."
                : "Tài khoản VanBanPlus trên máy này không còn hợp lệ (mã 401). Cần bấm «Đăng nhập» để đăng nhập lại.",
            MaLoiTacVu.KhongCoTacVu =>
                $"Máy chủ VanBanPlus chưa có tác vụ «{ten}» (mã 404). Cần cập nhật phần mềm lên bản mới nhất hoặc liên hệ hỗ trợ.",
            MaLoiTacVu.HetThoiGian => maHttp is not null
                ? $"Máy chủ phản hồi quá lâu (mã {maHttp}). Thử lại sau ít phút.{cau}"
                : $"Máy chủ phản hồi quá lâu (hơn {han.TotalSeconds:0} giây). Thử lại sau ít phút.{cau}",
            MaLoiTacVu.MatKetNoi => daGuiYeuCau
                ? $"Kết nối bị gián đoạn khi nhận kết quả từ máy chủ VanBanPlus. Kiểm tra kết nối Internet rồi thử lại.{cau}"
                : "Không kết nối được máy chủ VanBanPlus. Kiểm tra kết nối Internet rồi thử lại.",
            _ => maHttp is null or (>= 200 and < 300)
                ? $"Máy chủ VanBanPlus trả kết quả không đúng định dạng. Thử lại sau ít phút.{cau}"
                : $"Máy chủ VanBanPlus gặp lỗi (mã {maHttp}). Thử lại sau ít phút.{cau}",
        };

        return new TacVuException(ma, thongDiep, maHttp, thuLaiSau, chiTiet, goc);
    }
}
