using AIVanBan.API.Models;
using AIVanBan.API.Models.DTOs;

namespace AIVanBan.API.Services;

/// <summary>
/// Theo dõi usage & kiểm tra quota.
/// </summary>
public class UsageService
{
    private readonly DatabaseService _db;

    public UsageService(DatabaseService db)
    {
        _db = db;
    }

    /// <summary>
    /// Ghi nhận 1 lần gọi API.
    /// </summary>
    public void RecordUsage(UsageRecord record)
    {
        _db.Usage.Insert(record);
    }

    /// <summary>
    /// Kiểm tra user còn quota không (trả message lỗi nếu hết).
    /// </summary>
    public (bool allowed, string? errorMessage) CheckQuota(ApiUser user)
    {
        var plan = _db.Plans.FindById(user.SubscriptionPlanId);
        if (plan == null)
            return (false, "Gói subscription không hợp lệ.");

        // Check subscription expiry
        if (user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate < DateTime.UtcNow)
            return (false, "Gói subscription đã hết hạn. Vui lòng gia hạn.");

        // Unlimited plan
        if (plan.MaxRequestsPerMonth == -1)
            return (true, null);

        // Count this month's usage
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthlyUsage = _db.Usage
            .Find(u => u.UserId == user.Id && u.RequestDate >= startOfMonth && u.IsSuccess)
            .ToList();

        var requestCount = monthlyUsage.Count;
        var tokenCount = monthlyUsage.Sum(u => u.TotalTokens);

        if (requestCount >= plan.MaxRequestsPerMonth)
            return (false, $"Đã hết quota request tháng này ({requestCount}/{plan.MaxRequestsPerMonth}). Vui lòng nâng cấp gói.");

        if (plan.MaxTokensPerMonth > 0 && tokenCount >= plan.MaxTokensPerMonth)
            return (false, $"Đã hết quota token tháng này ({tokenCount:N0}/{plan.MaxTokensPerMonth:N0}). Vui lòng nâng cấp gói.");

        return (true, null);
    }

    /// <summary>
    /// Lấy tổng quan usage tháng hiện tại của user.
    /// </summary>
    public UsageSummary GetUsageSummary(string userId)
    {
        var user = _db.Users.FindById(userId);
        if (user == null) return new UsageSummary();

        var plan = _db.Plans.FindById(user.SubscriptionPlanId) 
            ?? SubscriptionPlan.GetDefaultPlans().First(p => p.Id == "free");

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthlyUsage = _db.Usage
            .Find(u => u.UserId == userId && u.RequestDate >= startOfMonth)
            .ToList();

        var successUsage = monthlyUsage.Where(u => u.IsSuccess).ToList();
        var requestsUsed = successUsage.Count;
        var tokensUsed = successUsage.Sum(u => u.TotalTokens);
        var costThisMonth = successUsage.Sum(u => u.EstimatedCost);

        return new UsageSummary
        {
            UserId = userId,
            PlanName = plan.Name,
            RequestsUsed = requestsUsed,
            RequestsLimit = plan.MaxRequestsPerMonth,
            TokensUsed = tokensUsed,
            TokensLimit = plan.MaxTokensPerMonth,
            RequestsPercent = plan.MaxRequestsPerMonth > 0 
                ? Math.Round((double)requestsUsed / plan.MaxRequestsPerMonth * 100, 1) : 0,
            TokensPercent = plan.MaxTokensPerMonth > 0 
                ? Math.Round((double)tokensUsed / plan.MaxTokensPerMonth * 100, 1) : 0,
            EstimatedCostThisMonth = costThisMonth,
            BillingPeriod = DateTime.UtcNow.ToString("MM/yyyy"),
            SubscriptionExpiry = user.SubscriptionEndDate
        };
    }

    /// <summary>
    /// Lấy usage theo ngày (30 ngày gần nhất).
    /// </summary>
    public List<DailyUsage> GetDailyUsage(string userId, int days = 30)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        var records = _db.Usage
            .Find(u => u.UserId == userId && u.RequestDate >= startDate)
            .ToList();

        return records
            .GroupBy(r => r.RequestDate.Date)
            .Select(g => new DailyUsage
            {
                Date = g.Key,
                Requests = g.Count(r => r.IsSuccess),
                Tokens = g.Where(r => r.IsSuccess).Sum(r => r.TotalTokens),
                Cost = g.Where(r => r.IsSuccess).Sum(r => r.EstimatedCost)
            })
            .OrderBy(d => d.Date)
            .ToList();
    }

    /// <summary>
    /// Thống kê tổng hợp cho admin dashboard.
    /// </summary>
    public AdminStats GetAdminStats()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfDay = now.Date;

        var allUsers = _db.Users.FindAll().ToList();
        var monthlyUsage = _db.Usage.Find(u => u.RequestDate >= startOfMonth).ToList();
        var dailyUsage = _db.Usage.Find(u => u.RequestDate >= startOfDay).ToList();

        return new AdminStats
        {
            TotalUsers = allUsers.Count,
            ActiveUsers = allUsers.Count(u => u.IsActive),
            UsersByPlan = allUsers
                .GroupBy(u => u.SubscriptionPlanId)
                .ToDictionary(g => g.Key, g => g.Count()),
            RequestsToday = dailyUsage.Count(u => u.IsSuccess),
            RequestsThisMonth = monthlyUsage.Count(u => u.IsSuccess),
            TokensThisMonth = monthlyUsage.Where(u => u.IsSuccess).Sum(u => u.TotalTokens),
            CostThisMonth = monthlyUsage.Where(u => u.IsSuccess).Sum(u => u.EstimatedCost),
            ErrorsToday = dailyUsage.Count(u => !u.IsSuccess),
            TopUsers = monthlyUsage
                .Where(u => u.IsSuccess)
                .GroupBy(u => u.UserId)
                .Select(g => new TopUserInfo
                {
                    UserId = g.Key,
                    UserName = allUsers.FirstOrDefault(u => u.Id == g.Key)?.FullName ?? "Unknown",
                    Email = allUsers.FirstOrDefault(u => u.Id == g.Key)?.Email ?? "",
                    Requests = g.Count(),
                    Tokens = g.Sum(r => r.TotalTokens)
                })
                .OrderByDescending(u => u.Requests)
                .Take(10)
                .ToList()
        };
    }
}

/// <summary>
/// Thống kê cho admin.
/// </summary>
public class AdminStats
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public Dictionary<string, int> UsersByPlan { get; set; } = new();
    public int RequestsToday { get; set; }
    public int RequestsThisMonth { get; set; }
    public int TokensThisMonth { get; set; }
    public decimal CostThisMonth { get; set; }
    public int ErrorsToday { get; set; }
    public List<TopUserInfo> TopUsers { get; set; } = new();
}

public class TopUserInfo
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public int Requests { get; set; }
    public int Tokens { get; set; }
}
