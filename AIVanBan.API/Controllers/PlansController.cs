using AIVanBan.API.Models;
using AIVanBan.API.Models.DTOs;
using AIVanBan.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIVanBan.API.Controllers;

/// <summary>
/// Plans Controller — Xem danh sách gói (public, không cần API key).
/// </summary>
[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly DatabaseService _db;

    public PlansController(DatabaseService db)
    {
        _db = db;
    }

    /// <summary>
    /// GET /api/plans — Danh sách gói subscription (public).
    /// </summary>
    [HttpGet]
    public IActionResult GetPlans()
    {
        var plans = _db.Plans.FindAll()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.MaxRequestsPerMonth,
                p.MaxTokensPerMonth,
                p.AllowStreaming,
                p.AllowVision,
                p.AllowDocumentGeneration,
                p.MaxFileSizeMB,
                p.PricePerMonth,
                p.PricePerYear
            });

        return Ok(ApiResponse<object>.Ok(plans));
    }
}
