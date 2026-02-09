namespace AIVanBan.API.Models;

/// <summary>
/// Gói subscription — mỗi gói có quota khác nhau.
/// </summary>
public class SubscriptionPlan
{
    public string Id { get; set; } = string.Empty; // free, basic, pro, enterprise
    
    public string Name { get; set; } = string.Empty; // "Miễn phí", "Cơ bản", "Chuyên nghiệp", "Doanh nghiệp"
    public string Description { get; set; } = string.Empty;
    
    // Giới hạn
    public int MaxRequestsPerMonth { get; set; } // Số request tối đa/tháng (-1 = unlimited)
    public int MaxTokensPerMonth { get; set; }    // Số token tối đa/tháng (-1 = unlimited)
    public int MaxTokensPerRequest { get; set; }  // Giới hạn mỗi request
    public int MaxFileSizeMB { get; set; }        // Giới hạn file upload (OCR)
    
    // Tính năng
    public bool AllowStreaming { get; set; }       // Cho phép streaming response
    public bool AllowVision { get; set; }          // Cho phép OCR/Vision (ảnh, PDF)
    public bool AllowDocumentGeneration { get; set; } // Cho phép tạo văn bản AI
    
    // Giá
    public decimal PricePerMonth { get; set; }     // VNĐ/tháng
    public decimal PricePerYear { get; set; }      // VNĐ/năm (giảm giá)
    
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    
    /// <summary>
    /// Tạo danh sách gói mặc định.
    /// </summary>
    public static List<SubscriptionPlan> GetDefaultPlans() => new()
    {
        new SubscriptionPlan
        {
            Id = "free",
            Name = "Miễn phí",
            Description = "Dùng thử — giới hạn 50 request/tháng",
            MaxRequestsPerMonth = 50,
            MaxTokensPerMonth = 100_000,
            MaxTokensPerRequest = 2048,
            MaxFileSizeMB = 5,
            AllowStreaming = false,
            AllowVision = true,
            AllowDocumentGeneration = true,
            PricePerMonth = 0,
            PricePerYear = 0,
            SortOrder = 0
        },
        new SubscriptionPlan
        {
            Id = "basic",
            Name = "Cơ bản",
            Description = "Cá nhân — 500 request/tháng",
            MaxRequestsPerMonth = 500,
            MaxTokensPerMonth = 1_000_000,
            MaxTokensPerRequest = 4096,
            MaxFileSizeMB = 10,
            AllowStreaming = true,
            AllowVision = true,
            AllowDocumentGeneration = true,
            PricePerMonth = 99_000,
            PricePerYear = 990_000,
            SortOrder = 1
        },
        new SubscriptionPlan
        {
            Id = "pro",
            Name = "Chuyên nghiệp",
            Description = "Đơn vị — 2000 request/tháng",
            MaxRequestsPerMonth = 2000,
            MaxTokensPerMonth = 5_000_000,
            MaxTokensPerRequest = 8192,
            MaxFileSizeMB = 20,
            AllowStreaming = true,
            AllowVision = true,
            AllowDocumentGeneration = true,
            PricePerMonth = 299_000,
            PricePerYear = 2_990_000,
            SortOrder = 2
        },
        new SubscriptionPlan
        {
            Id = "enterprise",
            Name = "Doanh nghiệp",
            Description = "Không giới hạn — liên hệ báo giá",
            MaxRequestsPerMonth = -1,
            MaxTokensPerMonth = -1,
            MaxTokensPerRequest = 16384,
            MaxFileSizeMB = 50,
            AllowStreaming = true,
            AllowVision = true,
            AllowDocumentGeneration = true,
            PricePerMonth = 999_000,
            PricePerYear = 9_990_000,
            SortOrder = 3
        }
    };
}
