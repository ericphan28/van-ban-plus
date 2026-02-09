using LiteDB;
using AIVanBan.API.Models;

namespace AIVanBan.API.Services;

/// <summary>
/// Database service — quản lý LiteDB cho API.
/// </summary>
public class DatabaseService : IDisposable
{
    private readonly LiteDatabase _db;

    public DatabaseService(IConfiguration config)
    {
        var dbPath = config.GetValue<string>("Database:Path") 
            ?? Path.Combine(AppContext.BaseDirectory, "Data", "vanbanplus-api.db");
        
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _db = new LiteDatabase(dbPath);
        
        // Indexes
        var users = _db.GetCollection<ApiUser>("users");
        users.EnsureIndex(x => x.Email, unique: true);
        users.EnsureIndex(x => x.ApiKey, unique: true);
        
        var usage = _db.GetCollection<UsageRecord>("usage");
        usage.EnsureIndex(x => x.UserId);
        usage.EnsureIndex(x => x.RequestDate);
        usage.EnsureIndex(x => x.ApiKey);
        
        var plans = _db.GetCollection<SubscriptionPlan>("plans");
        plans.EnsureIndex(x => x.Id, unique: true);
        
        // Seed default plans if empty
        if (plans.Count() == 0)
        {
            foreach (var plan in SubscriptionPlan.GetDefaultPlans())
                plans.Insert(plan);
        }
    }

    public ILiteCollection<ApiUser> Users => _db.GetCollection<ApiUser>("users");
    public ILiteCollection<UsageRecord> Usage => _db.GetCollection<UsageRecord>("usage");
    public ILiteCollection<SubscriptionPlan> Plans => _db.GetCollection<SubscriptionPlan>("plans");
    
    public void Dispose() => _db.Dispose();
}
