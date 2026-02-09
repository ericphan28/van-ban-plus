using AIVanBan.API.Models;
using AIVanBan.API.Services;

namespace AIVanBan.API.Middleware;

/// <summary>
/// Middleware xác thực API key từ header "X-API-Key".
/// Gắn user vào HttpContext.Items["ApiUser"] nếu hợp lệ.
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;

    // Các path KHÔNG cần API key
    private static readonly string[] PublicPaths = new[]
    {
        "/api/auth/register",
        "/api/auth/login",
        "/api/plans",
        "/swagger",
        "/health"
    };

    public ApiKeyAuthMiddleware(RequestDelegate next, ILogger<ApiKeyAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Skip auth for public paths
        if (PublicPaths.Any(p => path.StartsWith(p)))
        {
            await _next(context);
            return;
        }

        // Lấy API key từ header
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Thiếu API Key. Thêm header 'X-API-Key'." });
            return;
        }

        var apiKey = apiKeyHeader.ToString();
        var userService = context.RequestServices.GetRequiredService<UserService>();
        var user = userService.ValidateApiKey(apiKey);

        if (user == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "API Key không hợp lệ hoặc tài khoản đã bị khóa." });
            return;
        }

        // Gắn user vào context để controller dùng
        context.Items["ApiUser"] = user;

        // Kiểm tra admin routes
        if (path.StartsWith("/api/admin") && user.Role != UserRole.Admin)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Không có quyền truy cập." });
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Middleware kiểm tra quota trước khi cho gọi AI API.
/// </summary>
public class QuotaCheckMiddleware
{
    private readonly RequestDelegate _next;

    // Chỉ check quota cho các AI endpoint
    private static readonly string[] AiPaths = new[]
    {
        "/api/ai/"
    };

    public QuotaCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        if (AiPaths.Any(p => path.StartsWith(p)) && context.Items["ApiUser"] is ApiUser user)
        {
            var usageService = context.RequestServices.GetRequiredService<UsageService>();
            var (allowed, errorMessage) = usageService.CheckQuota(user);

            if (!allowed)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = errorMessage,
                    code = "QUOTA_EXCEEDED"
                });
                return;
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods để lấy ApiUser từ HttpContext.
/// </summary>
public static class HttpContextExtensions
{
    public static ApiUser? GetApiUser(this HttpContext context)
    {
        return context.Items["ApiUser"] as ApiUser;
    }

    public static string? GetClientIp(this HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
