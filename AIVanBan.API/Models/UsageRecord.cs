namespace AIVanBan.API.Models;

/// <summary>
/// Ghi nhận mỗi lần gọi API — để tracking usage.
/// </summary>
public class UsageRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string UserId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    
    // Request info
    public string Endpoint { get; set; } = string.Empty;     // /api/ai/generate, /api/ai/extract, ...
    public string RequestType { get; set; } = string.Empty;   // Generate, Extract, ReadText, Stream
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    
    // Token usage (từ Gemini response)
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
    
    // Cost estimation (VNĐ)
    public decimal EstimatedCost { get; set; }
    
    // Performance
    public int ResponseTimeMs { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    
    // Client info
    public string? ClientIp { get; set; }
    public string? ClientVersion { get; set; } // Version của desktop app
}
