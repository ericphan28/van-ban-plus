using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tích hợp AI — hỗ trợ 2 chế độ:
/// 1. VanBanPlus API (khuyến nghị): gọi qua API Gateway, quản lý quota/usage
/// 2. Gemini trực tiếp (legacy): gọi trực tiếp Google Gemini API
/// </summary>
public class GeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly bool _useVanBanPlusApi;
    private readonly string _vanBanPlusApiUrl;
    private readonly string _fallbackGeminiKey; // Fallback khi VanBanPlus lỗi
    private const string GEMINI_API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models";

    /// <summary>
    /// Khởi tạo từ AppSettings (khuyến nghị)
    /// </summary>
    public GeminiAIService()
    {
        var settings = AppSettingsService.Load();
        _useVanBanPlusApi = settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey);
        _vanBanPlusApiUrl = settings.VanBanPlusApiUrl.TrimEnd('/');
        _apiKey = _useVanBanPlusApi ? settings.VanBanPlusApiKey : settings.GeminiApiKey;
        _fallbackGeminiKey = settings.GeminiApiKey; // Dùng khi VanBanPlus lỗi
        
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(120); // Đặt sẵn 120s cho Vision/Extract
        
        if (_useVanBanPlusApi)
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            // Vercel Deployment Protection bypass (Hobby plan)
            var bypassToken = settings.VercelBypassToken;
            if (!string.IsNullOrEmpty(bypassToken))
                _httpClient.DefaultRequestHeaders.Add("x-vercel-protection-bypass", bypassToken);
        }
    }

    /// <summary>
    /// Khởi tạo truyền thẳng API Key (legacy, backward compatible)
    /// </summary>
    public GeminiAIService(string apiKey)
    {
        var settings = AppSettingsService.Load();
        
        // Nếu có VanBanPlus config, ưu tiên dùng nó
        if (settings.UseVanBanPlusApi && !string.IsNullOrEmpty(settings.VanBanPlusApiKey))
        {
            _useVanBanPlusApi = true;
            _vanBanPlusApiUrl = settings.VanBanPlusApiUrl.TrimEnd('/');
            _apiKey = settings.VanBanPlusApiKey;
            _fallbackGeminiKey = !string.IsNullOrEmpty(settings.GeminiApiKey) ? settings.GeminiApiKey : apiKey;
        }
        else
        {
            _useVanBanPlusApi = false;
            _vanBanPlusApiUrl = "";
            _apiKey = apiKey;
            _fallbackGeminiKey = apiKey;
        }
        
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(120); // Đặt sẵn 120s cho Vision/Extract
        
        if (_useVanBanPlusApi)
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            // Vercel Deployment Protection bypass (Hobby plan)
            var bypassToken = settings.VercelBypassToken;
            if (!string.IsNullOrEmpty(bypassToken))
                _httpClient.DefaultRequestHeaders.Add("x-vercel-protection-bypass", bypassToken);
        }
    }

    /// <summary>
    /// Đang dùng VanBanPlus API hay Gemini trực tiếp?
    /// </summary>
    public bool IsUsingVanBanPlusApi => _useVanBanPlusApi;

    /// <summary>
    /// Lấy Gemini API Key cho gọi trực tiếp (ưu tiên fallback key)
    /// </summary>
    private string GeminiDirectKey => _useVanBanPlusApi ? _fallbackGeminiKey : _apiKey;

    /// <summary>
    /// Tạo nội dung văn bản từ prompt
    /// </summary>
    public async Task<string> GenerateContentAsync(string prompt, string? systemInstruction = null)
    {
        try
        {
            // ===== VanBanPlus API mode =====
            if (_useVanBanPlusApi)
            {
                try
                {
                    var body = new
                    {
                        prompt = prompt,
                        systemInstruction = systemInstruction
                    };
                    var vbpResponse = await _httpClient.PostAsJsonAsync($"{_vanBanPlusApiUrl}/api/ai/generate", body);
                    vbpResponse.EnsureSuccessStatusCode();
                    var vbpResult = await vbpResponse.Content.ReadFromJsonAsync<VanBanPlusAIResponse>();
                    return vbpResult?.Data?.Content ?? "Không thể tạo nội dung";
                }
                catch (Exception ex) when (!string.IsNullOrEmpty(_fallbackGeminiKey))
                {
                    // Fallback sang Gemini trực tiếp
                    Console.WriteLine($"⚠️ VanBanPlus API lỗi, fallback Gemini: {ex.Message}");
                }
            }

            // ===== Gemini trực tiếp (legacy) =====
            var requestBody = new GeminiRequest
            {
                Contents = new[]
                {
                    new Content
                    {
                        Parts = new[] { new Part { Text = prompt } }
                    }
                },
                GenerationConfig = new GenerationConfig { Temperature = 0.7, MaxOutputTokens = 16384 }
            };

            // Thêm system instruction nếu có
            if (!string.IsNullOrEmpty(systemInstruction))
            {
                requestBody.SystemInstruction = new Content
                {
                    Parts = new[] { new Part { Text = systemInstruction } }
                };
            }

            var url = $"{GEMINI_API_BASE_URL}/gemini-2.5-flash:generateContent?key={GeminiDirectKey}";
            
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            
            if (result?.Candidates != null && result.Candidates.Length > 0)
            {
                // Gemini 2.5 trả nhiều parts (thinking + answer) → lấy part cuối
                var gParts = result.Candidates[0].Content?.Parts;
                var text = (gParts != null && gParts.Length > 0)
                    ? gParts[gParts.Length - 1]?.Text ?? ""
                    : "";
                return text;
            }

            return "Không thể tạo nội dung";
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi gọi AI: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tạo nội dung văn bản với streaming (realtime)
    /// </summary>
    public async IAsyncEnumerable<string> GenerateContentStreamAsync(string prompt, string? systemInstruction = null)
    {
        // VanBanPlus API mode: không hỗ trợ streaming → trả về toàn bộ 1 lần
        if (_useVanBanPlusApi)
        {
            var result = await GenerateContentAsync(prompt, systemInstruction);
            yield return result;
            yield break;
        }

        // ===== Gemini trực tiếp: streaming =====
        var requestBody = new GeminiRequest
        {
            Contents = new[]
            {
                new Content
                {
                    Parts = new[] { new Part { Text = prompt } }
                }
            }
        };

        if (!string.IsNullOrEmpty(systemInstruction))
        {
            requestBody.SystemInstruction = new Content
            {
                Parts = new[] { new Part { Text = systemInstruction } }
            };
        }

        var url = $"{GEMINI_API_BASE_URL}/gemini-2.5-flash:streamGenerateContent?key={GeminiDirectKey}&alt=sse";
        
        var jsonContent = JsonContent.Create(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = jsonContent
        };

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var jsonData = line.Substring(6); // Remove "data: " prefix
            if (jsonData == "[DONE]")
                break;

            GeminiResponse? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<GeminiResponse>(jsonData);
            }
            catch
            {
                continue;
            }

            if (chunk?.Candidates != null && chunk.Candidates.Length > 0)
            {
                var text = chunk.Candidates[0].Content?.Parts?[0]?.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }

    #region PDF/Image OCR with Vision

    /// <summary>
    /// DTO chứa dữ liệu văn bản trích xuất từ ảnh/PDF scan
    /// </summary>
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
        public string LinhVuc { get; set; } = "";
        public string DiaDanh { get; set; } = ""; // Địa danh ban hành (VD: Gia Kiểm, Biên Hòa, Hà Nội)
        public string ChucDanhKy { get; set; } = ""; // Chức danh người ký (VD: CHỦ TỊCH, GIÁM ĐỐC)
        public string ThamQuyenKy { get; set; } = ""; // Thẩm quyền ký (VD: TM., KT., Q., hoặc rỗng)
    }

    /// <summary>
    /// Trích xuất thông tin văn bản hành chính từ file ảnh hoặc PDF scan bằng Gemini Vision
    /// </summary>
    public async Task<ExtractedDocumentData> ExtractDocumentFromFileAsync(string filePath)
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var base64 = Convert.ToBase64String(fileBytes);
        
        var ext = Path.GetExtension(filePath).ToLower();
        var mimeType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".tiff" or ".tif" => "image/tiff",
            _ => "application/octet-stream"
        };

        return await ExtractDocumentFromBytesAsync(base64, mimeType);
    }

    /// <summary>
    /// Trích xuất từ dữ liệu base64
    /// </summary>
    public async Task<ExtractedDocumentData> ExtractDocumentFromBytesAsync(string base64Data, string mimeType)
    {
        try
        {
            // ===== VanBanPlus API mode =====
            if (_useVanBanPlusApi)
            {
                try
                {
                    var body = new { base64Data, mimeType };
                    var vbpResponse = await _httpClient.PostAsJsonAsync($"{_vanBanPlusApiUrl}/api/ai/extract", body);
                    vbpResponse.EnsureSuccessStatusCode();
                    var vbpResult = await vbpResponse.Content.ReadFromJsonAsync<VanBanPlusAIResponse>();
                    var text = vbpResult?.Data?.Content ?? "";
                    return ParseExtractedDocument(text);
                }
                catch (Exception ex) when (!string.IsNullOrEmpty(_fallbackGeminiKey))
                {
                    Console.WriteLine($"⚠️ VanBanPlus API lỗi, fallback Gemini: {ex.Message}");
                }
            }

            // ===== Gemini trực tiếp (legacy) =====
            var prompt = @"Bạn là chuyên gia phân tích văn bản hành chính Việt Nam. 
Hãy đọc kỹ toàn bộ nội dung văn bản trong file/ảnh này và trích xuất thông tin chi tiết.

YÊU CẦU QUAN TRỌNG:
- Đọc TOÀN BỘ nội dung, không bỏ sót
- Giữ nguyên dấu tiếng Việt chính xác
- Số văn bản phải đúng format (VD: 15/GM-UBND, 234/CV-SGDĐT)
- Ngày tháng format dd/MM/yyyy
- loai_van_ban chỉ chọn 1 trong: CongVan, QuyetDinh, BaoCao, ToTrinh, KeHoach, ThongBao, NghiQuyet, ChiThi, HuongDan, Khac
- huong_van_ban chỉ chọn 1 trong: Den, Di, NoiBo
- Nội dung phải đầy đủ, không tóm tắt (nếu văn bản DÀI thì tóm tắt tối đa 2000 ký tự)
- Output là JSON theo đúng schema đã khai báo";

            // JSON Schema cho Structured Output — Gemini đảm bảo 100% valid JSON
            var extractSchema = new
            {
                type = "object",
                properties = new Dictionary<string, object>
                {
                    ["so_van_ban"] = new { type = "string", description = "Số hiệu văn bản (VD: 15/GM-UBND)" },
                    ["trich_yeu"] = new { type = "string", description = "Trích yếu nội dung văn bản" },
                    ["loai_van_ban"] = new { type = "string", description = "Loại văn bản", @enum = new[] { "CongVan", "QuyetDinh", "BaoCao", "ToTrinh", "KeHoach", "ThongBao", "NghiQuyet", "ChiThi", "HuongDan", "Khac" } },
                    ["ngay_ban_hanh"] = new { type = "string", description = "Ngày ban hành dd/MM/yyyy" },
                    ["co_quan_ban_hanh"] = new { type = "string", description = "Tên cơ quan ban hành" },
                    ["nguoi_ky"] = new { type = "string", description = "Họ tên người ký" },
                    ["noi_dung"] = new { type = "string", description = "Nội dung chính văn bản. Ngắn thì đầy đủ, dài thì tóm tắt tối đa 2000 ký tự" },
                    ["noi_nhan"] = new { type = "array", items = new { type = "string" }, description = "Danh sách nơi nhận" },
                    ["can_cu"] = new { type = "array", items = new { type = "string" }, description = "Danh sách căn cứ pháp lý" },
                    ["huong_van_ban"] = new { type = "string", description = "Hướng văn bản", @enum = new[] { "Den", "Di", "NoiBo" } },
                    ["linh_vuc"] = new { type = "string", description = "Lĩnh vực liên quan (VD: Giáo dục, Y tế, Tài chính)" },
                    ["dia_danh"] = new { type = "string", description = "Địa danh nơi ban hành (VD: Gia Kiểm, Biên Hòa, Hà Nội)" },
                    ["chuc_danh_ky"] = new { type = "string", description = "Chức danh người ký (VD: CHỦ TỊCH, GIÁM ĐỐC)" },
                    ["tham_quyen_ky"] = new { type = "string", description = "Thẩm quyền ký (TM., KT., Q. hoặc rỗng)" }
                },
                required = new[] { "so_van_ban", "trich_yeu", "loai_van_ban", "ngay_ban_hanh", "co_quan_ban_hanh", "nguoi_ky", "noi_dung", "noi_nhan", "can_cu", "huong_van_ban", "linh_vuc", "dia_danh", "chuc_danh_ky", "tham_quyen_ky" }
            };

            var requestBody = new GeminiRequest
            {
                Contents = new[]
                {
                    new Content
                    {
                        Parts = new[]
                        {
                            new Part { Text = prompt },
                            new Part
                            {
                                InlineData = new InlineData
                                {
                                    MimeType = mimeType,
                                    Data = base64Data
                                }
                            }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0.1,
                    MaxOutputTokens = 65536,
                    ResponseMimeType = "application/json",
                    ResponseSchema = extractSchema,
                    ThinkingConfig = new ThinkingConfig { ThinkingBudget = 0 }
                }
            };

            var url = $"{GEMINI_API_BASE_URL}/gemini-2.5-flash:generateContent?key={GeminiDirectKey}";

            var jsonOptions = new JsonSerializerOptions 
            { 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            };
            var json = JsonSerializer.Serialize(requestBody, jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            if (result?.Candidates != null && result.Candidates.Length > 0)
            {
                var candidate = result.Candidates[0];
                var parts = candidate.Content?.Parts;
                
                // Log chi tiết response
                Console.WriteLine($"📊 Gemini Direct Extract — finishReason: {candidate.FinishReason}, parts: {parts?.Length ?? 0}");
                if (result.UsageMetadata != null)
                {
                    Console.WriteLine($"📊 Tokens — prompt: {result.UsageMetadata.PromptTokenCount}, completion: {result.UsageMetadata.CandidatesTokenCount}, total: {result.UsageMetadata.TotalTokenCount}");
                }

                // Với Structured Output + thinkingBudget=0 → chỉ có 1 part duy nhất chứa JSON hợp lệ
                var text = (parts != null && parts.Length > 0)
                    ? parts[parts.Length - 1]?.Text ?? ""
                    : "";
                
                Console.WriteLine($"📊 Content length: {text.Length}, preview: {(text.Length > 200 ? text[..200] + "..." : text)}");
                
                return ParseExtractedDocument(text);
            }

            return new ExtractedDocumentData();
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi trích xuất văn bản bằng AI: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Đọc nội dung text thuần từ PDF/ảnh scan (không trích xuất metadata)
    /// </summary>
    public async Task<string> ReadTextFromFileAsync(string filePath)
    {
        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var base64 = Convert.ToBase64String(fileBytes);
        
        var ext = Path.GetExtension(filePath).ToLower();
        var mimeType = ext switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        // ===== VanBanPlus API mode =====
        if (_useVanBanPlusApi)
        {
            try
            {
                var body = new { base64Data = base64, mimeType };
                var vbpResponse = await _httpClient.PostAsJsonAsync($"{_vanBanPlusApiUrl}/api/ai/read-text", body);
                vbpResponse.EnsureSuccessStatusCode();
                var vbpResult = await vbpResponse.Content.ReadFromJsonAsync<VanBanPlusAIResponse>();
                return vbpResult?.Data?.Content ?? "";
            }
            catch (Exception ex) when (!string.IsNullOrEmpty(_fallbackGeminiKey))
            {
                Console.WriteLine($"⚠️ VanBanPlus API lỗi, fallback Gemini: {ex.Message}");
            }
        }

        // ===== Gemini trực tiếp (legacy) =====
        var prompt = @"Đọc và trả về TOÀN BỘ nội dung text trong file/ảnh này.
Giữ nguyên format, xuống dòng, dấu tiếng Việt. 
CHỈ trả về nội dung text, KHÔNG thêm giải thích hay markdown.";

        var requestBody = new GeminiRequest
        {
            Contents = new[]
            {
                new Content
                {
                    Parts = new[]
                    {
                        new Part { Text = prompt },
                        new Part { InlineData = new InlineData { MimeType = mimeType, Data = base64 } }
                    }
                }
            },
            GenerationConfig = new GenerationConfig { Temperature = 0.1, MaxOutputTokens = 16384 }
        };

        var url = $"{GEMINI_API_BASE_URL}/gemini-2.5-flash:generateContent?key={GeminiDirectKey}";
        var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var json = JsonSerializer.Serialize(requestBody, jsonOptions);
        var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, httpContent);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
        // Gemini 2.5 trả nhiều parts (thinking + answer) → lấy part cuối
        var resultParts = result?.Candidates?[0]?.Content?.Parts;
        return (resultParts != null && resultParts.Length > 0)
            ? resultParts[resultParts.Length - 1]?.Text ?? ""
            : "";
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
                LinhVuc = GetJsonString(root, "linh_vuc"),
                DiaDanh = GetJsonString(root, "dia_danh"),
                ChucDanhKy = GetJsonString(root, "chuc_danh_ky"),
                ThamQuyenKy = GetJsonString(root, "tham_quyen_ky")
            };

            if (root.TryGetProperty("noi_nhan", out var noiNhan) && noiNhan.ValueKind == JsonValueKind.Array)
                result.NoiNhan = noiNhan.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray();

            if (root.TryGetProperty("can_cu", out var canCu) && canCu.ValueKind == JsonValueKind.Array)
                result.CanCu = canCu.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToArray();

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

    private string GetJsonString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? "";
        return "";
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

    #endregion

    #region DTOs cho Gemini API

    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public Content[] Contents { get; set; } = Array.Empty<Content>();

        [JsonPropertyName("systemInstruction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Content? SystemInstruction { get; set; }

        [JsonPropertyName("generationConfig")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public GenerationConfig? GenerationConfig { get; set; }
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; } = Array.Empty<Part>();

        [JsonPropertyName("role")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Role { get; set; }
    }

    private class Part
    {
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }

        [JsonPropertyName("inline_data")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public InlineData? InlineData { get; set; }
    }

    private class InlineData
    {
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = "";

        [JsonPropertyName("data")]
        public string Data { get; set; } = ""; // base64
    }

    private class GenerationConfig
    {
        [JsonPropertyName("temperature")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? MaxOutputTokens { get; set; }

        [JsonPropertyName("responseMimeType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ResponseMimeType { get; set; }

        [JsonPropertyName("responseSchema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? ResponseSchema { get; set; }

        [JsonPropertyName("thinkingConfig")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ThinkingConfig? ThinkingConfig { get; set; }
    }

    private class ThinkingConfig
    {
        [JsonPropertyName("thinkingBudget")]
        public int ThinkingBudget { get; set; }
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public UsageMetadata? UsageMetadata { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    private class UsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }

    #endregion

    #region DTOs cho VanBanPlus API

    private class VanBanPlusAIResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public VanBanPlusAIData? Data { get; set; }
    }

    private class VanBanPlusAIData
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        [JsonPropertyName("promptTokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completionTokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("totalTokens")]
        public int TotalTokens { get; set; }
    }

    #endregion
}
