using AIVanBan.API.Models;
using AIVanBan.API.Models.DTOs;
using AIVanBan.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIVanBan.API.Controllers;

/// <summary>
/// Admin Controller — Quản trị hệ thống (chỉ admin mới truy cập được).
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly UserService _userService;
    private readonly UsageService _usageService;
    private readonly DatabaseService _db;

    public AdminController(UserService userService, UsageService usageService, DatabaseService db)
    {
        _userService = userService;
        _usageService = usageService;
        _db = db;
    }

    /// <summary>
    /// GET /api/admin/stats — Thống kê tổng hợp.
    /// </summary>
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var stats = _usageService.GetAdminStats();
        return Ok(ApiResponse<AdminStats>.Ok(stats));
    }

    /// <summary>
    /// GET /api/admin/users — Danh sách tất cả users.
    /// </summary>
    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        var users = _userService.GetAllUsers().Select(u => new
        {
            u.Id,
            u.Email,
            u.FullName,
            u.Phone,
            u.Company,
            u.Role,
            u.IsActive,
            Plan = u.SubscriptionPlanId,
            u.SubscriptionStartDate,
            u.SubscriptionEndDate,
            u.CreatedDate,
            u.LastLoginDate,
            u.LastLoginIp,
            Usage = _usageService.GetUsageSummary(u.Id)
        });

        return Ok(ApiResponse<object>.Ok(users));
    }

    /// <summary>
    /// GET /api/admin/users/{userId}/usage — Chi tiết usage 1 user.
    /// </summary>
    [HttpGet("users/{userId}/usage")]
    public IActionResult GetUserUsage(string userId, [FromQuery] int days = 30)
    {
        var user = _userService.GetById(userId);
        if (user == null) return NotFound(ApiResponse<object>.Fail("User không tồn tại."));

        return Ok(ApiResponse<object>.Ok(new
        {
            User = new { user.Id, user.Email, user.FullName, Plan = user.SubscriptionPlanId },
            Summary = _usageService.GetUsageSummary(userId),
            Daily = _usageService.GetDailyUsage(userId, days)
        }));
    }

    /// <summary>
    /// PUT /api/admin/users/{userId}/plan — Cập nhật gói subscription.
    /// </summary>
    [HttpPut("users/{userId}/plan")]
    public IActionResult UpdatePlan(string userId, [FromBody] UpdatePlanRequest request)
    {
        var success = _userService.UpdateSubscription(userId, request.PlanId, request.EndDate);
        if (!success) return NotFound(ApiResponse<object>.Fail("User không tồn tại."));

        return Ok(ApiResponse<object>.Ok(new { Message = $"Đã cập nhật gói '{request.PlanId}' cho user." }));
    }

    /// <summary>
    /// PUT /api/admin/users/{userId}/active — Khóa/mở khóa tài khoản.
    /// </summary>
    [HttpPut("users/{userId}/active")]
    public IActionResult SetActive(string userId, [FromBody] SetActiveRequest request)
    {
        var success = _userService.SetUserActive(userId, request.IsActive);
        if (!success) return NotFound(ApiResponse<object>.Fail("User không tồn tại."));

        var action = request.IsActive ? "mở khóa" : "khóa";
        return Ok(ApiResponse<object>.Ok(new { Message = $"Đã {action} tài khoản." }));
    }

    /// <summary>
    /// GET /api/admin/plans — Danh sách gói subscription.
    /// </summary>
    [HttpGet("plans")]
    public IActionResult GetPlans()
    {
        var plans = _db.Plans.FindAll().OrderBy(p => p.SortOrder).ToList();
        return Ok(ApiResponse<List<SubscriptionPlan>>.Ok(plans));
    }
}

public class UpdatePlanRequest
{
    public string PlanId { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
}

public class SetActiveRequest
{
    public bool IsActive { get; set; }
}
