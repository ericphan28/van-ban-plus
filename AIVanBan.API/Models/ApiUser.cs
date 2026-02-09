namespace AIVanBan.API.Models;

/// <summary>
/// Người dùng API — mỗi khách hàng là 1 ApiUser.
/// </summary>
public class ApiUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Thông tin cơ bản
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty; // Tên cơ quan/đơn vị
    public string PasswordHash { get; set; } = string.Empty;
    
    // API Key — dùng để gọi API từ desktop app
    public string ApiKey { get; set; } = GenerateApiKey();
    
    // Phân quyền
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    
    // Subscription
    public string SubscriptionPlanId { get; set; } = "free"; // free, basic, pro, enterprise
    public DateTime SubscriptionStartDate { get; set; } = DateTime.UtcNow;
    public DateTime? SubscriptionEndDate { get; set; }
    
    // Audit
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIp { get; set; }
    
    /// <summary>
    /// Tạo API key ngẫu nhiên: vbp_xxxxxxxxxxxxxxxxxxxx
    /// </summary>
    public static string GenerateApiKey()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return "vbp_" + Convert.ToHexString(bytes).ToLower().Substring(0, 32);
    }
}

public enum UserRole
{
    User,       // Khách hàng thường
    Admin       // Quản trị viên
}
