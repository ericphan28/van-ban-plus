using AIVanBan.API.Middleware;
using AIVanBan.API.Models.DTOs;
using AIVanBan.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIVanBan.API.Controllers;

/// <summary>
/// Auth Controller — Đăng ký, đăng nhập, quản lý API key.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// POST /api/auth/register — Đăng ký tài khoản mới.
    /// </summary>
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        var result = _userService.Register(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// POST /api/auth/login — Đăng nhập, nhận API key.
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = _userService.Login(request);
        if (!result.Success) return Unauthorized(result);

        // Update IP
        var user = _userService.GetById(result.Data!.UserId);
        if (user != null)
        {
            user.LastLoginIp = HttpContext.GetClientIp();
            // Save is already done in Login()
        }

        return Ok(result);
    }

    /// <summary>
    /// GET /api/auth/me — Lấy thông tin user hiện tại (cần API key).
    /// </summary>
    [HttpGet("me")]
    public IActionResult GetProfile()
    {
        var user = HttpContext.GetApiUser();
        if (user == null) return Unauthorized();

        return Ok(ApiResponse<object>.Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            user.Company,
            user.ApiKey,
            Plan = user.SubscriptionPlanId,
            user.SubscriptionStartDate,
            user.SubscriptionEndDate,
            user.CreatedDate,
            user.LastLoginDate
        }));
    }

    /// <summary>
    /// POST /api/auth/regenerate-key — Tạo API key mới.
    /// </summary>
    [HttpPost("regenerate-key")]
    public IActionResult RegenerateKey()
    {
        var user = HttpContext.GetApiUser();
        if (user == null) return Unauthorized();

        var newKey = _userService.RegenerateApiKey(user.Id);
        return Ok(ApiResponse<object>.Ok(new { ApiKey = newKey }, "API key đã được tạo lại. Hãy cập nhật trong app."));
    }
}
