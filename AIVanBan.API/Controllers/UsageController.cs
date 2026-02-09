using AIVanBan.API.Middleware;
using AIVanBan.API.Models.DTOs;
using AIVanBan.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIVanBan.API.Controllers;

/// <summary>
/// Usage Controller — Xem thông tin sử dụng & quota.
/// </summary>
[ApiController]
[Route("api/usage")]
public class UsageController : ControllerBase
{
    private readonly UsageService _usageService;

    public UsageController(UsageService usageService)
    {
        _usageService = usageService;
    }

    /// <summary>
    /// GET /api/usage/summary — Tổng quan usage tháng hiện tại.
    /// </summary>
    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        var user = HttpContext.GetApiUser();
        if (user == null) return Unauthorized();

        var summary = _usageService.GetUsageSummary(user.Id);
        return Ok(ApiResponse<UsageSummary>.Ok(summary));
    }

    /// <summary>
    /// GET /api/usage/daily?days=30 — Usage theo ngày (biểu đồ).
    /// </summary>
    [HttpGet("daily")]
    public IActionResult GetDailyUsage([FromQuery] int days = 30)
    {
        var user = HttpContext.GetApiUser();
        if (user == null) return Unauthorized();

        var daily = _usageService.GetDailyUsage(user.Id, days);
        return Ok(ApiResponse<List<DailyUsage>>.Ok(daily));
    }
}
