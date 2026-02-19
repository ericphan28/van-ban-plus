using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIVanBan.API.Models;
using AIVanBan.API.Models.DTOs;

namespace AIVanBan.API.Services;

/// <summary>
/// Proxy service — gọi Gemini API thay cho client.
/// Gemini API key được giấu ở server, client không bao giờ thấy.
/// </summary>
public class GeminiProxyService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly UsageService _usageService;
    private readonly ILogger<GeminiProxyService> _logger;
    private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models";

    // Cost estimation: Gemini 2.5 Flash (Paid Tier 1)
    // Input: $0.30 / 1M tokens × 25,300 VNĐ/USD = 7,590 VNĐ / 1M tokens
    // Output (incl. thinking): $2.50 / 1M tokens × 25,300 VNĐ/USD = 63,250 VNĐ / 1M tokens
    // Ref: https://ai.google.dev/gemini-api/docs/pricing#gemini-2.5-flash
    private const decimal COST_PER_INPUT_TOKEN = 0.00759m;   // VNĐ
    private const decimal COST_PER_OUTPUT_TOKEN = 0.06325m;  // VNĐ

    public GeminiProxyService(IConfiguration config, UsageService usageService, ILogger<GeminiProxyService> logger)
    {
        _apiKey = config.GetValue<string>("Gemini:ApiKey") 
            ?? throw new Exception("Gemini:ApiKey chưa được cấu hình trong appsettings.json");
        _model = config.GetValue<string>("Gemini:Model") ?? "gemini-2.5-flash";
        _usageService = usageService;
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
    }

    /// <summary>
    /// Generate text content — proxy tới Gemini.
    /// </summary>
    public async Task<ApiResponse<GenerateResponse>> GenerateAsync(GenerateRequest request, ApiUser user, string? clientIp)
    {
        var sw = Stopwatch.StartNew();
        var usageRecord = new UsageRecord
        {
            UserId = user.Id,
            ApiKey = user.ApiKey,
            Endpoint = "/api/ai/generate",
            RequestType = "Generate",
            ClientIp = clientIp
        };

        try
        {
            var plan = GetUserPlan(user);
            var maxTokens = request.MaxTokens ?? plan?.MaxTokensPerRequest ?? 4096;

            var body = new GeminiRequestBody
            {
                Contents = new[]
                {
                    new GeminiContent
                    {
                        Parts = new[] { new GeminiPart { Text = request.Prompt } }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = request.Temperature ?? 0.7,
                    MaxOutputTokens = maxTokens
                }
            };

            if (!string.IsNullOrEmpty(request.SystemInstruction))
            {
                body.SystemInstruction = new GeminiContent
                {
                    Parts = new[] { new GeminiPart { Text = request.SystemInstruction } }
                };
            }

            var url = $"{API_BASE_URL}/{_model}:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, body, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponseBody>();
            sw.Stop();

            var text = result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "";
            var promptTokens = result?.UsageMetadata?.PromptTokenCount ?? 0;
            var completionTokens = result?.UsageMetadata?.CandidatesTokenCount ?? 0;
            var totalTokens = result?.UsageMetadata?.TotalTokenCount ?? 0;

            // Record usage
            usageRecord.PromptTokens = promptTokens;
            usageRecord.CompletionTokens = completionTokens;
            usageRecord.TotalTokens = totalTokens;
            usageRecord.EstimatedCost = promptTokens * COST_PER_INPUT_TOKEN + completionTokens * COST_PER_OUTPUT_TOKEN;
            usageRecord.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            usageRecord.IsSuccess = true;
            _usageService.RecordUsage(usageRecord);

            return ApiResponse<GenerateResponse>.Ok(new GenerateResponse
            {
                Content = text,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            usageRecord.IsSuccess = false;
            usageRecord.ErrorMessage = ex.Message;
            usageRecord.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            _usageService.RecordUsage(usageRecord);

            _logger.LogError(ex, "Gemini API error for user {UserId}", user.Id);
            return ApiResponse<GenerateResponse>.Fail($"Lỗi gọi AI: {ex.Message}");
        }
    }

    /// <summary>
    /// Extract document from image/PDF — proxy tới Gemini Vision.
    /// </summary>
    public async Task<ApiResponse<GenerateResponse>> ExtractDocumentAsync(ExtractDocumentRequest request, ApiUser user, string? clientIp)
    {
        var sw = Stopwatch.StartNew();
        var usageRecord = new UsageRecord
        {
            UserId = user.Id,
            ApiKey = user.ApiKey,
            Endpoint = "/api/ai/extract",
            RequestType = "Extract",
            ClientIp = clientIp
        };

        try
        {
            var prompt = @"Bạn là chuyên gia phân tích văn bản hành chính Việt Nam.
Hãy đọc kỹ toàn bộ nội dung văn bản trong file/ảnh này và trích xuất thông tin chi tiết.

YÊU CẦU: Đọc TOÀN BỘ nội dung, giữ nguyên dấu tiếng Việt. Trả về JSON thuần (không markdown):
{
  ""so_van_ban"": """", ""trich_yeu"": """", ""loai_van_ban"": """",
  ""ngay_ban_hanh"": ""dd/MM/yyyy"", ""co_quan_ban_hanh"": """",
  ""nguoi_ky"": """", ""noi_dung"": """",
  ""noi_nhan"": [], ""can_cu"": [],
  ""huong_van_ban"": ""Den/Di/NoiBo"", ""linh_vuc"": """",
  ""dia_danh"": """", ""chuc_danh_ky"": """", ""tham_quyen_ky"": """"
}";

            var body = new GeminiRequestBody
            {
                Contents = new[]
                {
                    new GeminiContent
                    {
                        Parts = new[]
                        {
                            new GeminiPart { Text = prompt },
                            new GeminiPart
                            {
                                InlineData = new GeminiInlineData
                                {
                                    MimeType = request.MimeType,
                                    Data = request.Base64Data
                                }
                            }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = 0.1,
                    MaxOutputTokens = 8192
                }
            };

            var url = $"{API_BASE_URL}/{_model}:generateContent?key={_apiKey}";
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponseBody>();
            sw.Stop();

            var text = result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "";
            var promptTokens = result?.UsageMetadata?.PromptTokenCount ?? 0;
            var completionTokens = result?.UsageMetadata?.CandidatesTokenCount ?? 0;
            var totalTokens = result?.UsageMetadata?.TotalTokenCount ?? 0;

            usageRecord.PromptTokens = promptTokens;
            usageRecord.CompletionTokens = completionTokens;
            usageRecord.TotalTokens = totalTokens;
            usageRecord.EstimatedCost = promptTokens * COST_PER_INPUT_TOKEN + completionTokens * COST_PER_OUTPUT_TOKEN;
            usageRecord.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            usageRecord.IsSuccess = true;
            _usageService.RecordUsage(usageRecord);

            return ApiResponse<GenerateResponse>.Ok(new GenerateResponse
            {
                Content = text,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            usageRecord.IsSuccess = false;
            usageRecord.ErrorMessage = ex.Message;
            usageRecord.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            _usageService.RecordUsage(usageRecord);

            _logger.LogError(ex, "Gemini Vision API error for user {UserId}", user.Id);
            return ApiResponse<GenerateResponse>.Fail($"Lỗi trích xuất văn bản: {ex.Message}");
        }
    }

    /// <summary>
    /// Đọc text thuần từ file — proxy tới Gemini Vision.
    /// </summary>
    public async Task<ApiResponse<GenerateResponse>> ReadTextAsync(ReadTextRequest request, ApiUser user, string? clientIp)
    {
        var sw = Stopwatch.StartNew();
        var usageRecord = new UsageRecord
        {
            UserId = user.Id,
            ApiKey = user.ApiKey,
            Endpoint = "/api/ai/read-text",
            RequestType = "ReadText",
            ClientIp = clientIp
        };

        try
        {
            var prompt = @"Đọc và trả về TOÀN BỘ nội dung text trong file/ảnh này.
Giữ nguyên format, xuống dòng, dấu tiếng Việt. CHỈ trả về nội dung text, KHÔNG thêm giải thích hay markdown.";

            var body = new GeminiRequestBody
            {
                Contents = new[]
                {
                    new GeminiContent
                    {
                        Parts = new[]
                        {
                            new GeminiPart { Text = prompt },
                            new GeminiPart
                            {
                                InlineData = new GeminiInlineData
                                {
                                    MimeType = request.MimeType,
                                    Data = request.Base64Data
                                }
                            }
                        }
                    }
                },
                GenerationConfig = new GeminiGenerationConfig
                {
                    Temperature = 0.1,
                    MaxOutputTokens = 8192
                }
            };

            var url = $"{API_BASE_URL}/{_model}:generateContent?key={_apiKey}";
            var json = JsonSerializer.Serialize(body, _jsonOptions);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GeminiResponseBody>();
            sw.Stop();

            var text = result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "";
            var promptTokens = result?.UsageMetadata?.PromptTokenCount ?? 0;
            var completionTokens = result?.UsageMetadata?.CandidatesTokenCount ?? 0;
            var totalTokens = result?.UsageMetadata?.TotalTokenCount ?? 0;

            usageRecord.PromptTokens = promptTokens;
            usageRecord.CompletionTokens = completionTokens;
            usageRecord.TotalTokens = totalTokens;
            usageRecord.EstimatedCost = promptTokens * COST_PER_INPUT_TOKEN + completionTokens * COST_PER_OUTPUT_TOKEN;
            usageRecord.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            usageRecord.IsSuccess = true;
            _usageService.RecordUsage(usageRecord);

            return ApiResponse<GenerateResponse>.Ok(new GenerateResponse
            {
                Content = text,
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = totalTokens
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            usageRecord.IsSuccess = false;
            usageRecord.ErrorMessage = ex.Message;
            usageRecord.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            _usageService.RecordUsage(usageRecord);

            _logger.LogError(ex, "Gemini ReadText API error for user {UserId}", user.Id);
            return ApiResponse<GenerateResponse>.Fail($"Lỗi đọc text: {ex.Message}");
        }
    }

    private SubscriptionPlan? GetUserPlan(ApiUser user)
    {
        // Access DatabaseService through UsageService would be circular,
        // so we use default plans as fallback
        return SubscriptionPlan.GetDefaultPlans().FirstOrDefault(p => p.Id == user.SubscriptionPlanId);
    }

    // ============================================================
    // Gemini API DTOs (private)
    // ============================================================

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private class GeminiRequestBody
    {
        [JsonPropertyName("contents")]
        public GeminiContent[] Contents { get; set; } = Array.Empty<GeminiContent>();

        [JsonPropertyName("systemInstruction")]
        public GeminiContent? SystemInstruction { get; set; }

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = Array.Empty<GeminiPart>();
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("inline_data")]
        public GeminiInlineData? InlineData { get; set; }
    }

    private class GeminiInlineData
    {
        [JsonPropertyName("mime_type")]
        public string MimeType { get; set; } = "";

        [JsonPropertyName("data")]
        public string Data { get; set; } = "";
    }

    private class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int? MaxOutputTokens { get; set; }
    }

    private class GeminiResponseBody
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private class GeminiUsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int CandidatesTokenCount { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }
}
