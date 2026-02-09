using AIVanBan.API.Models;
using AIVanBan.API.Models.DTOs;
using BCrypt.Net;

namespace AIVanBan.API.Services;

/// <summary>
/// Quản lý user: đăng ký, đăng nhập, xác thực API key.
/// </summary>
public class UserService
{
    private readonly DatabaseService _db;

    public UserService(DatabaseService db)
    {
        _db = db;
    }

    /// <summary>
    /// Đăng ký tài khoản mới.
    /// </summary>
    public ApiResponse<AuthResponse> Register(RegisterRequest request)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(request.Email))
            return ApiResponse<AuthResponse>.Fail("Email không được để trống.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            return ApiResponse<AuthResponse>.Fail("Mật khẩu phải có ít nhất 6 ký tự.");
        if (string.IsNullOrWhiteSpace(request.FullName))
            return ApiResponse<AuthResponse>.Fail("Họ tên không được để trống.");

        // Check duplicate email
        var existing = _db.Users.FindOne(u => u.Email == request.Email.ToLower().Trim());
        if (existing != null)
            return ApiResponse<AuthResponse>.Fail("Email đã được sử dụng.");

        var user = new ApiUser
        {
            Email = request.Email.ToLower().Trim(),
            FullName = request.FullName.Trim(),
            Phone = request.Phone?.Trim() ?? "",
            Company = request.Company?.Trim() ?? "",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            SubscriptionPlanId = "free",
            CreatedDate = DateTime.UtcNow
        };

        _db.Users.Insert(user);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            ApiKey = user.ApiKey,
            SubscriptionPlan = user.SubscriptionPlanId
        }, "Đăng ký thành công! Hãy lưu lại API Key.");
    }

    /// <summary>
    /// Đăng nhập bằng email + password → trả về API key.
    /// </summary>
    public ApiResponse<AuthResponse> Login(LoginRequest request)
    {
        var user = _db.Users.FindOne(u => u.Email == request.Email.ToLower().Trim());
        if (user == null)
            return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không đúng.");
        
        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail("Tài khoản đã bị khóa.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không đúng.");

        // Update last login
        user.LastLoginDate = DateTime.UtcNow;
        _db.Users.Update(user);

        return ApiResponse<AuthResponse>.Ok(new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            ApiKey = user.ApiKey,
            SubscriptionPlan = user.SubscriptionPlanId
        });
    }

    /// <summary>
    /// Xác thực API key → trả về user (null nếu không hợp lệ).
    /// </summary>
    public ApiUser? ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        var user = _db.Users.FindOne(u => u.ApiKey == apiKey);
        if (user == null || !user.IsActive) return null;
        return user;
    }

    /// <summary>
    /// Tạo lại API key mới cho user.
    /// </summary>
    public string RegenerateApiKey(string userId)
    {
        var user = _db.Users.FindById(userId);
        if (user == null) throw new Exception("User không tồn tại.");
        
        user.ApiKey = ApiUser.GenerateApiKey();
        _db.Users.Update(user);
        return user.ApiKey;
    }

    /// <summary>
    /// Lấy user theo ID.
    /// </summary>
    public ApiUser? GetById(string userId) => _db.Users.FindById(userId);

    /// <summary>
    /// Lấy tất cả users (admin).
    /// </summary>
    public List<ApiUser> GetAllUsers() => _db.Users.FindAll().ToList();

    /// <summary>
    /// Cập nhật subscription plan cho user.
    /// </summary>
    public bool UpdateSubscription(string userId, string planId, DateTime? endDate = null)
    {
        var user = _db.Users.FindById(userId);
        if (user == null) return false;

        user.SubscriptionPlanId = planId;
        user.SubscriptionStartDate = DateTime.UtcNow;
        user.SubscriptionEndDate = endDate;
        return _db.Users.Update(user);
    }

    /// <summary>
    /// Khóa/mở khóa tài khoản (admin).
    /// </summary>
    public bool SetUserActive(string userId, bool isActive)
    {
        var user = _db.Users.FindById(userId);
        if (user == null) return false;
        user.IsActive = isActive;
        return _db.Users.Update(user);
    }

    /// <summary>
    /// Tạo admin account mặc định nếu chưa có.
    /// </summary>
    public void EnsureAdminExists(string adminEmail, string adminPassword)
    {
        var admin = _db.Users.FindOne(u => u.Role == UserRole.Admin);
        if (admin != null) return;

        var user = new ApiUser
        {
            Email = adminEmail.ToLower().Trim(),
            FullName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRole.Admin,
            SubscriptionPlanId = "enterprise",
            IsActive = true
        };
        _db.Users.Insert(user);
    }
}
