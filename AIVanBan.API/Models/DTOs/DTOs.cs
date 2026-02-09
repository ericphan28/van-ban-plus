namespace AIVanBan.API.Models.DTOs;

// ============================================================
// REQUEST DTOs — Desktop app gửi lên
// ============================================================

/// <summary>
/// Request tạo nội dung AI (generate text).
/// </summary>
public class GenerateRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? SystemInstruction { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
}

/// <summary>
/// Request trích xuất văn bản từ file (OCR/Vision).
/// </summary>
public class ExtractDocumentRequest
{
    public string Base64Data { get; set; } = string.Empty;  // File content as base64
    public string MimeType { get; set; } = string.Empty;    // application/pdf, image/jpeg, ...
    public string? FileName { get; set; }
}

/// <summary>
/// Request đọc text thuần từ file.
/// </summary>
public class ReadTextRequest
{
    public string Base64Data { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
}

/// <summary>
/// Request đăng ký tài khoản.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Company { get; set; }
}

/// <summary>
/// Request đăng nhập.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


// ============================================================
// RESPONSE DTOs — API trả về cho desktop app
// ============================================================

/// <summary>
/// Response chung cho mọi API.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    
    public static ApiResponse<T> Ok(T data, string message = "Thành công") => new()
    {
        Success = true,
        Message = message,
        Data = data
    };
    
    public static ApiResponse<T> Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}

/// <summary>
/// Response sau khi đăng nhập/đăng ký thành công.
/// </summary>
public class AuthResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = string.Empty;
}

/// <summary>
/// Kết quả generate AI text.
/// </summary>
public class GenerateResponse
{
    public string Content { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public int TotalTokens { get; set; }
}

/// <summary>
/// Thông tin usage hiện tại của user.
/// </summary>
public class UsageSummary
{
    public string UserId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    
    // Tháng hiện tại
    public int RequestsUsed { get; set; }
    public int RequestsLimit { get; set; }   // -1 = unlimited
    public int TokensUsed { get; set; }
    public int TokensLimit { get; set; }     // -1 = unlimited
    
    // Phần trăm
    public double RequestsPercent { get; set; }
    public double TokensPercent { get; set; }
    
    // Chi phí ước tính
    public decimal EstimatedCostThisMonth { get; set; }
    
    // Thời gian
    public string BillingPeriod { get; set; } = string.Empty; // "02/2026"
    public DateTime? SubscriptionExpiry { get; set; }
}

/// <summary>
/// Thông tin chi tiết từng ngày.
/// </summary>
public class DailyUsage
{
    public DateTime Date { get; set; }
    public int Requests { get; set; }
    public int Tokens { get; set; }
    public decimal Cost { get; set; }
}
