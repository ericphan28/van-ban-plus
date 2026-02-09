using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIVanBan.Core.Models;

namespace AIVanBan.Core.Services;

/// <summary>
/// Service tích hợp Gemini API để tạo nội dung văn bản tự động
/// </summary>
public class GeminiAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiAIService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
    }

    /// <summary>
    /// Tạo nội dung văn bản từ prompt
    /// </summary>
    public async Task<string> GenerateContentAsync(string prompt, string? systemInstruction = null)
    {
        try
        {
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

            // Thêm system instruction nếu có
            if (!string.IsNullOrEmpty(systemInstruction))
            {
                requestBody.SystemInstruction = new Content
                {
                    Parts = new[] { new Part { Text = systemInstruction } }
                };
            }

            var url = $"{API_BASE_URL}/gemini-2.5-flash:generateContent?key={_apiKey}";
            
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            
            if (result?.Candidates != null && result.Candidates.Length > 0)
            {
                var text = result.Candidates[0].Content?.Parts?[0]?.Text ?? "";
                return text;
            }

            return "Không thể tạo nội dung";
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi gọi Gemini API: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Tạo nội dung văn bản với streaming (realtime)
    /// </summary>
    public async IAsyncEnumerable<string> GenerateContentStreamAsync(string prompt, string? systemInstruction = null)
    {
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

        var url = $"{API_BASE_URL}/gemini-2.5-flash:streamGenerateContent?key={_apiKey}&alt=sse";
        
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
            var prompt = @"Bạn là chuyên gia phân tích văn bản hành chính Việt Nam. 
Hãy đọc kỹ toàn bộ nội dung văn bản trong file/ảnh này và trích xuất thông tin chi tiết.

YÊU CẦU QUAN TRỌNG:
- Đọc TOÀN BỘ nội dung, không bỏ sót
- Giữ nguyên dấu tiếng Việt chính xác
- Số văn bản phải đúng format (VD: 15/GM-UBND, 234/CV-SGDĐT)
- Ngày tháng format dd/MM/yyyy
- Nội dung phải đầy đủ, không tóm tắt

Trả về JSON (KHÔNG markdown, KHÔNG ```json```, chỉ thuần JSON):
{
  ""so_van_ban"": ""số hiệu văn bản (VD: 15/GM-UBND)"",
  ""trich_yeu"": ""trích yếu nội dung văn bản"",
  ""loai_van_ban"": ""một trong: CongVan, QuyetDinh, BaoCao, ToTrinh, KeHoach, ThongBao, NghiQuyet, ChiThi, HuongDan, Khac"",
  ""ngay_ban_hanh"": ""dd/MM/yyyy"",
  ""co_quan_ban_hanh"": ""tên cơ quan ban hành"",
  ""nguoi_ky"": ""họ tên người ký"",
  ""noi_dung"": ""toàn bộ nội dung chính của văn bản (đầy đủ, không tóm tắt)"",
  ""noi_nhan"": [""nơi nhận 1"", ""nơi nhận 2""],
  ""can_cu"": [""căn cứ pháp lý 1"", ""căn cứ pháp lý 2""],
  ""huong_van_ban"": ""Den hoặc Di hoặc NoiBo"",
  ""linh_vuc"": ""lĩnh vực liên quan (VD: Giáo dục, Y tế, Tài chính...)"",
  ""dia_danh"": ""địa danh nơi ban hành văn bản, lấy từ dòng ngày tháng bên phải (VD: Gia Kiểm, Biên Hòa, Đồng Nai, Hà Nội). KHÔNG bịa đặt, chỉ lấy đúng từ văn bản"",
  ""chuc_danh_ky"": ""chức danh/chức vụ của người ký (VD: CHỦ TỊCH, GIÁM ĐỐC, TRƯỞNG PHÒNG). Lấy chính xác từ phần ký tên cuối văn bản"",
  ""tham_quyen_ky"": ""thẩm quyền ký, là dòng chữ nhỏ phía trên chức danh (VD: TM. nếu có 'TM.', KT. nếu có 'KT.', Q. nếu có 'Q.'). Nếu người ký trực tiếp không qua ủy quyền thì để rỗng""
}";

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
                    MaxOutputTokens = 8192
                }
            };

            var url = $"{API_BASE_URL}/gemini-2.5-flash:generateContent?key={_apiKey}";

            var jsonOptions = new JsonSerializerOptions 
            { 
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
            };
            var json = JsonSerializer.Serialize(requestBody, jsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.Timeout = TimeSpan.FromSeconds(120); // Vision cần thời gian lâu hơn
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            if (result?.Candidates != null && result.Candidates.Length > 0)
            {
                var text = result.Candidates[0].Content?.Parts?[0]?.Text ?? "";
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
            GenerationConfig = new GenerationConfig { Temperature = 0.1, MaxOutputTokens = 8192 }
        };

        var url = $"{API_BASE_URL}/gemini-2.5-flash:generateContent?key={_apiKey}";
        var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        var json = JsonSerializer.Serialize(requestBody, jsonOptions);
        var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        _httpClient.Timeout = TimeSpan.FromSeconds(120);
        var response = await _httpClient.PostAsync(url, httpContent);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
        return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "";
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
        catch
        {
            // Nếu parse JSON thất bại, trả về với nội dung raw
            return new ExtractedDocumentData { NoiDung = jsonText };
        }
    }

    private string GetJsonString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? "";
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
}
