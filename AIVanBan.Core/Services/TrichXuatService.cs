using System.IO;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace AIVanBan.Core.Services;

/// <summary>
/// Trích xuất văn bản từ ảnh/PDF scan qua tác vụ v2 trich-xuat (chế độ cau-truc hoặc chi-chu).
/// Prompt OCR và khóa AI nằm ở máy chủ VanBanPlus; bản cài máy không gọi thẳng nhà cung cấp AI.
/// </summary>
public class TrichXuatService
{
    /// <summary>Hợp đồng: mỗi lần gửi 1..20 tệp.</summary>
    public const int SoTepToiDa = 20;

    /// <summary>Hợp đồng: tổng dung lượng tệp (sau giải base64) tối đa 20 MB; gói có thể thấp hơn, máy chủ trả QUA_DAI.</summary>
    public const long TongByteToiDa = 20L * 1024 * 1024;

    /// <summary>Định dạng máy chủ nhận; mọi định dạng khác phải đổi sang PNG trước khi gửi.</summary>
    private static readonly HashSet<string> MimeMayChuNhan = new(StringComparer.Ordinal)
    {
        "application/pdf", "image/png", "image/jpeg", "image/webp", "image/heic", "image/heif",
    };

    /// <summary>Định dạng ảnh máy chủ không nhận nhưng WPF đọc được: đổi sang PNG.</summary>
    private static readonly HashSet<string> MimeDoiSangPng = new(StringComparer.Ordinal)
    {
        "image/gif", "image/bmp", "image/x-ms-bmp", "image/tiff",
    };

    private readonly TacVuClient _tacVu;

    public TrichXuatService() : this(new TacVuClient()) { }

    public TrichXuatService(TacVuClient tacVu)
    {
        _tacVu = tacVu;
    }

    /// <summary>Credit đã trừ, số dư, cờ bị cắt của lượt chạy gần nhất.</summary>
    public ThongTinLuot? LuotCuoi { get; private set; }

    /// <summary>DTO chứa dữ liệu văn bản trích xuất từ ảnh/PDF scan (tên trường JSON snake_case ở ParseExtractedDocument).</summary>
    public class ExtractedDocumentData
    {
        public string SoVanBan { get; set; } = "";
        public string TrichYeu { get; set; } = "";
        public string LoaiVanBan { get; set; } = "";
        public string NgayBanHanh { get; set; } = "";
        public string CoQuanBanHanh { get; set; } = "";
        public string NguoiKy { get; set; } = "";
        public string NoiDung { get; set; } = "";
        public string[] NoiNhan { get; set; } = Array.Empty<string>();
        public string[] CanCu { get; set; } = Array.Empty<string>();
        public string HuongVanBan { get; set; } = ""; // Đi/Đến/Nội bộ
        public string DoKhan { get; set; } = "Thuong"; // Thuong/Khan/ThuongKhan/HoaToc
        public string DoMat { get; set; } = "Thuong"; // Thuong/Mat/ToiMat/TuyetMat
        public string LinhVuc { get; set; } = "";
        public string DiaDanh { get; set; } = "";
        public string ChucDanhKy { get; set; } = "";
        public string ThamQuyenKy { get; set; } = "";
    }

    /// <summary>
    /// Đọc tệp thành base64 + mime theo đuôi tệp. GIF/BMP/TIFF đổi sang PNG (máy chủ không nhận);
    /// đuôi lạ ném <see cref="ArgumentException"/>.
    /// </summary>
    public static (string Base64, string MimeType) DocTep(string filePath)
    {
        var mime = Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".tiff" or ".tif" => "image/tiff",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            _ => "",
        };
        return ChuanBiTep(File.ReadAllBytes(filePath), mime);
    }

    /// <summary>
    /// Đưa tệp về định dạng máy chủ nhận: PDF/PNG/JPEG/WEBP/HEIC/HEIF giữ nguyên; GIF/BMP/TIFF đổi sang PNG
    /// (chỉ khung hình đầu tiên); định dạng khác ném <see cref="ArgumentException"/>.
    /// </summary>
    public static (string Base64, string MimeType) ChuanBiTep(byte[] bytes, string mimeType)
    {
        var mime = (mimeType ?? "").ToLowerInvariant();
        if (MimeMayChuNhan.Contains(mime))
            return (Convert.ToBase64String(bytes), mime);
        if (MimeDoiSangPng.Contains(mime))
            return (Convert.ToBase64String(ChuyenSangPng(bytes)), "image/png");
        throw new ArgumentException("Định dạng tệp không được hỗ trợ. Hỗ trợ: PDF, JPG, PNG, WebP, HEIC, BMP, GIF, TIFF.");
    }

    private static byte[] ChuyenSangPng(byte[] bytes)
    {
        try
        {
            using var vao = new MemoryStream(bytes);
            var bo = BitmapDecoder.Create(vao, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bo.Frames[0]));
            using var ra = new MemoryStream();
            encoder.Save(ra);
            return ra.ToArray();
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Không đọc được tệp ảnh để đổi sang PNG: {ex.Message}", ex);
        }
    }

    /// <summary>Ước tính theo số tệp và dung lượng; thân giống hệt lượt chạy.</summary>
    public Task<UocTinhKetQua> UocTinhAsync(IReadOnlyList<(string Base64, string MimeType)> tep, bool chiChu = false)
        => _tacVu.UocTinhAsync("trich-xuat", DauVao(tep, chiChu));

    /// <summary>Nhiều tệp là các TRANG của cùng một văn bản → một kết quả (một lượt chạy).</summary>
    public async Task<ExtractedDocumentData> ExtractDocumentFromMultipleFilesAsync(IReadOnlyList<(string Base64, string MimeType)> files)
    {
        var kq = await _tacVu.ChayAsync<JsonElement>("trich-xuat", DauVao(files, chiChu: false));
        LuotCuoi = kq.Luot;
        // Bị cắt: máy chủ trả chuỗi thô (hợp đồng) → ParseExtractedDocument vớt từng trường như bản cũ; bên gọi báo «bị cắt».
        if (kq.ChuoiTho != null)
            return ParseExtractedDocument(kq.ChuoiTho);
        var ketQua = kq.KetQua;
        return ParseExtractedDocument(ketQua.ValueKind == JsonValueKind.String
            ? ketQua.GetString() ?? ""
            : ketQua.GetRawText());
    }

    public Task<ExtractedDocumentData> ExtractDocumentFromBytesAsync(string base64Data, string mimeType)
        => ExtractDocumentFromMultipleFilesAsync(new[] { (base64Data, mimeType) });

    public Task<ExtractedDocumentData> ExtractDocumentFromFileAsync(string filePath)
    {
        var (base64, mime) = DocTep(filePath);
        return ExtractDocumentFromBytesAsync(base64, mime);
    }

    /// <summary>Chỉ đọc chữ (không trích trường), chế độ chi-chu, đúng 1 tệp.</summary>
    public async Task<string> ReadTextAsync(string base64Data, string mimeType)
    {
        var kq = await _tacVu.ChayAsync<string>("trich-xuat", DauVao(new[] { (base64Data, mimeType) }, chiChu: true));
        LuotCuoi = kq.Luot;
        return kq.KetQua;
    }

    public Task<string> ReadTextFromFileAsync(string filePath)
    {
        var (base64, mime) = DocTep(filePath);
        return ReadTextAsync(base64, mime);
    }

    private static object DauVao(IReadOnlyList<(string Base64, string MimeType)> tep, bool chiChu)
    {
        if (tep.Count == 0)
            throw new ArgumentException("Cần ít nhất 1 tệp để trích xuất.");
        if (tep.Count > SoTepToiDa)
            throw new ArgumentException($"Mỗi lần chỉ gửi tối đa {SoTepToiDa} tệp. Cần chia thành nhiều lần.");
        if (chiChu && tep.Count != 1)
            throw new ArgumentException("Chế độ đọc chữ chỉ nhận đúng 1 tệp mỗi lần.");
        foreach (var t in tep)
        {
            if (!MimeMayChuNhan.Contains(t.MimeType))
                throw new ArgumentException("Định dạng tệp không được hỗ trợ. Hỗ trợ: PDF, JPG, PNG, WebP, HEIC.");
        }
        var tong = tep.Sum(t => SoByteGiai(t.Base64));
        if (tong > TongByteToiDa)
            throw new ArgumentException(
                $"Tổng dung lượng tệp {tong / (1024.0 * 1024):F1} MB vượt giới hạn {TongByteToiDa / (1024 * 1024)} MB mỗi lần trích xuất. Cần bớt tệp hoặc giảm dung lượng.");
        return new
        {
            tep = tep.Select(t => new { base64 = t.Base64, mime = t.MimeType }).ToArray(),
            che_do = chiChu ? "chi-chu" : "cau-truc",
        };
    }

    /// <summary>Số byte sau giải base64, không giải thật.</summary>
    private static long SoByteGiai(string base64)
    {
        var s = base64.AsSpan().TrimEnd();
        var dem = s.Length / 4 * 3L;
        if (s.Length >= 2 && s[^1] == '=')
            dem -= s[^2] == '=' ? 2 : 1;
        return dem;
    }

    private ExtractedDocumentData ParseExtractedDocument(string jsonText)
    {
        try
        {
            // Clean up - remove markdown code fences if present
            jsonText = jsonText.Trim();
            if (jsonText.StartsWith("```"))
            {
                var firstNewline = jsonText.IndexOf('\n');
                if (firstNewline > 0)
                    jsonText = jsonText.Substring(firstNewline + 1);
            }
            if (jsonText.EndsWith("```"))
                jsonText = jsonText.Substring(0, jsonText.Length - 3);
            jsonText = jsonText.Trim();

            // Nếu text không bắt đầu bằng '{', thử tìm JSON block trong text
            if (!jsonText.StartsWith("{"))
            {
                var jsonStart = jsonText.IndexOf('{');
                var jsonEnd = jsonText.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    jsonText = jsonText.Substring(jsonStart, jsonEnd - jsonStart + 1);
                }
            }

            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            var result = new ExtractedDocumentData
            {
                SoVanBan = GetJsonString(root, "so_van_ban"),
                TrichYeu = GetJsonString(root, "trich_yeu"),
                LoaiVanBan = GetJsonString(root, "loai_van_ban"),
                NgayBanHanh = GetJsonString(root, "ngay_ban_hanh"),
                CoQuanBanHanh = GetJsonString(root, "co_quan_ban_hanh"),
                NguoiKy = GetJsonString(root, "nguoi_ky"),
                NoiDung = GetJsonString(root, "noi_dung"),
                HuongVanBan = GetJsonString(root, "huong_van_ban"),
                DoKhan = GetJsonString(root, "do_khan", "Thuong"),
                DoMat = GetJsonString(root, "do_mat", "Thuong"),
                LinhVuc = GetJsonString(root, "linh_vuc"),
                DiaDanh = GetJsonString(root, "dia_danh"),
                ChucDanhKy = GetJsonString(root, "chuc_danh_ky"),
                ThamQuyenKy = GetJsonString(root, "tham_quyen_ky")
            };

            if (root.TryGetProperty("noi_nhan", out var noiNhan) && noiNhan.ValueKind == JsonValueKind.Array)
                result.NoiNhan = noiNhan.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray();

            if (root.TryGetProperty("can_cu", out var canCu) && canCu.ValueKind == JsonValueKind.Array)
                result.CanCu = canCu.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray();

            // Post-process: đảm bảo nội dung có line breaks đúng cấu trúc văn bản
            result.NoiDung = FormatExtractedContent(result.NoiDung);

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ParseExtractedDocument FAILED: {ex.Message}");
            Console.WriteLine($"❌ Text (first 500 chars): {jsonText?.Substring(0, Math.Min(500, jsonText?.Length ?? 0))}");
            
            // Thử salvage: nếu JSON bị cắt ngang, trích xuất từng field bằng regex
            var salvaged = new ExtractedDocumentData();
            try
            {
                salvaged.SoVanBan = ExtractJsonField(jsonText, "so_van_ban");
                salvaged.TrichYeu = ExtractJsonField(jsonText, "trich_yeu");
                salvaged.LoaiVanBan = ExtractJsonField(jsonText, "loai_van_ban");
                salvaged.NgayBanHanh = ExtractJsonField(jsonText, "ngay_ban_hanh");
                salvaged.CoQuanBanHanh = ExtractJsonField(jsonText, "co_quan_ban_hanh");
                salvaged.NguoiKy = ExtractJsonField(jsonText, "nguoi_ky");
                salvaged.HuongVanBan = ExtractJsonField(jsonText, "huong_van_ban");
                salvaged.DoKhan = ExtractJsonField(jsonText, "do_khan");
                salvaged.DoMat = ExtractJsonField(jsonText, "do_mat");
                salvaged.LinhVuc = ExtractJsonField(jsonText, "linh_vuc");
                salvaged.DiaDanh = ExtractJsonField(jsonText, "dia_danh");
                salvaged.ChucDanhKy = ExtractJsonField(jsonText, "chuc_danh_ky");
                salvaged.ThamQuyenKy = ExtractJsonField(jsonText, "tham_quyen_ky");
                // noi_dung có thể rất dài và bị cắt → lấy best effort
                salvaged.NoiDung = ExtractJsonField(jsonText, "noi_dung");
                if (string.IsNullOrEmpty(salvaged.NoiDung))
                    salvaged.NoiDung = jsonText; // fallback raw text
            }
            catch
            {
                salvaged.NoiDung = jsonText;
            }
            return salvaged;
        }
    }

    /// <summary>
    /// Post-process nội dung trích xuất: thêm line breaks cho cấu trúc văn bản hành chính
    /// Gemini đôi khi trả về 1 khối text liền → cần tách thành các đoạn rõ ràng
    /// </summary>
    private string FormatExtractedContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return content;

        // Bước 1: Loại bỏ tiêu đề văn bản nếu Gemini vô tình đưa vào noi_dung
        content = StripHeaderFromContent(content);

        // Bước 2: Nếu đã có nhiều line breaks (>= 3 dòng), Gemini đã format tốt → giữ nguyên
        var existingLineBreaks = content.Split('\n').Length;
        if (existingLineBreaks >= 3) return content;

        // Gemini gộp hết thành 1 dòng → cần tách ra
        // Pattern: thêm \n trước các keyword cấu trúc văn bản hành chính VN
        var patterns = new[]
        {
            // Chương, Phần, Mục (cấp cao nhất)
            @"(?<=[.;:])\s*(?=Chương\s+[IVXLCDM\d]+)",
            @"(?<=[.;:])\s*(?=CHƯƠNG\s+[IVXLCDM\d]+)",
            @"(?<=[.;:])\s*(?=Phần\s+(thứ\s+)?[IVXLCDM\d]+)",
            @"(?<=[.;:])\s*(?=PHẦN\s+(THỨ\s+)?[IVXLCDM\d]+)",
            @"(?<=[.;:])\s*(?=Mục\s+\d+)",
            @"(?<=[.;:])\s*(?=MỤC\s+\d+)",
            // Điều (quan trọng nhất trong Luật/NĐ/QĐ)
            @"(?<=[.;:])\s*(?=Điều\s+\d+)",
            // Khoản (dùng số + dấu chấm)
            @"(?<=[.;:])\s*(?=\d+\.\s+[A-ZÀÁẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬĐÈÉẺẼẸÊẾỀỂỄỆÌÍỈĨỊÒÓỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢÙÚỦŨỤƯỨỪỬỮỰỲÝỶỸỴ])",
            // Điểm a), b), c)...
            @"(?<=[.;:])\s*(?=[a-zđ]\)\s)",
        };

        var result = content;
        foreach (var pattern in patterns)
        {
            result = System.Text.RegularExpressions.Regex.Replace(result, pattern, "\n");
        }

        // Trim các dòng thừa
        var lines = result.Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l));
        
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Loại bỏ các dòng tiêu đề văn bản khỏi noi_dung (nếu Gemini vô tình đưa vào)
    /// Ví dụ: QUỐC HỘI, CỘNG HÒA XÃ HỘI..., Độc lập - Tự do, Luật số..., LUẬT, QUẢN LÝ THUẾ
    /// </summary>
    private string StripHeaderFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return content;
        
        var lines = content.Split('\n').ToList();
        
        // Các pattern dòng tiêu đề cần loại bỏ (đầu văn bản)
        var headerPatterns = new[]
        {
            @"^\s*QUỐC HỘI\s*$",
            @"^\s*CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT\s*NAM\s*$",
            @"^\s*Độc lập\s*-\s*Tự do\s*-\s*Hạnh phúc\s*$",
            @"^\s*(Luật|Nghị định|Quyết định|Thông tư|Công văn)\s+số[:\s]",  // Luật số: 108/...
            @"^\s*Số[:\s]+\d+",  // Số: 108/2025/QH15
            @"^\s*Ngày\s+\d+\s+tháng\s+\d+",  // Ngày 10 tháng 12 năm 2025
            @"^\s*QUYẾ?T ĐỊN?H\s*[:\s]*$",  // QUYẾ?T ĐỊNH:
            @"^\s*LUẬT\s*$",  // LUẬT
            @"^\s*NGHỊ ĐỊNH\s*$",
            @"^\s*THÔNG TƯ\s*$",
            @"^\s*CHỦ TỊCH\s",  // CHỦ TỊCH QUỐC HỘI
        };

        // Chỉ loại bỏ các dòng ở ĐẦU văn bản (trước Chương/Điều đầu tiên)
        int firstContentLine = 0;
        for (int i = 0; i < lines.Count && i < 20; i++) // Chỉ check 20 dòng đầu
        {
            var trimmed = lines[i].Trim();
            if (string.IsNullOrEmpty(trimmed)) 
            {
                firstContentLine = i + 1;
                continue;
            }

            // Nếu gặp Chương/Điều đầu tiên → đây là bắt đầu nội dung chính
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^(Chương|CHƯƠNG)\s+[IVXLCDM\d]") ||
                System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^Điều\s+\d+"))
            {
                firstContentLine = i;
                break;
            }

            // Kiểm tra có phải header line không
            bool isHeader = headerPatterns.Any(p => 
                System.Text.RegularExpressions.Regex.IsMatch(trimmed, p, System.Text.RegularExpressions.RegexOptions.IgnoreCase));
            
            // Kiểm tra dòng trích yếu/tên luật viết HOA toàn bộ và ngắn (VD: QUẢN LÝ THUẾ)
            bool isShortUpperCase = trimmed.Length <= 60 && 
                trimmed == trimmed.ToUpper() && 
                !trimmed.StartsWith("CHƯƠNG") && !trimmed.StartsWith("ĐIỀU") &&
                !trimmed.StartsWith("QUY ĐỊNH");
            
            if (isHeader || isShortUpperCase)
            {
                firstContentLine = i + 1;
            }
            else if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^Căn cứ\s", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                // Bỏ qua dòng căn cứ (đã có trường can_cu)
                firstContentLine = i + 1;
            }
            else
            {
                // Gặp dòng nội dung bình thường → dừng strip
                firstContentLine = i;
                break;
            }
        }

        if (firstContentLine > 0 && firstContentLine < lines.Count)
        {
            lines = lines.Skip(firstContentLine).ToList();
        }

        return string.Join("\n", lines).Trim();
    }

    private string GetJsonString(JsonElement root, string propertyName, string defaultValue = "")
    {
        if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? defaultValue;
        return defaultValue;
    }

    /// <summary>
    /// Trích xuất giá trị field từ JSON text bằng regex (dùng khi JSON bị cắt/invalid)
    /// </summary>
    private string ExtractJsonField(string text, string fieldName)
    {
        if (string.IsNullOrEmpty(text)) return "";
        // Match: "field_name": "value" (handle escaped quotes inside value)
        var pattern = $@"""{fieldName}""\s*:\s*""((?:[^""\\]|\\.)*)""";
        var match = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
        if (match.Success && match.Groups.Count > 1)
        {
            return match.Groups[1].Value
                .Replace("\\n", "\n")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }
        return "";
    }
}
