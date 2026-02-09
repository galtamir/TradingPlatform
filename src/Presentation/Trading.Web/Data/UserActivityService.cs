using Microsoft.EntityFrameworkCore;

namespace Trading.Web.Data;

public sealed class UserActivityService(
    ApplicationDbContext db,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserActivityService> logger) : IUserActivityService
{
    public async Task LogAsync(string userId, string email, string action, string? detail = null)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var activity = new UserActivity
        {
            UserId = userId,
            UserEmail = email,
            Action = action,
            Detail = detail,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString()?[..Math.Min(
                httpContext?.Request.Headers.UserAgent.ToString().Length ?? 0, 512)],
            Timestamp = DateTime.UtcNow
        };

        db.UserActivities.Add(activity);
        await db.SaveChangesAsync();
        logger.LogInformation("Activity: {Action} by {Email}", action, email);
    }

    public async Task<List<UserActivity>> GetRecentAsync(int count = 50)
    {
        return await db.UserActivities
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<UserActivity>> GetByUserAsync(string userId, int count = 50)
    {
        return await db.UserActivities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetActionSummaryAsync(int days = 30)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return await db.UserActivities
            .Where(a => a.Timestamp >= since)
            .GroupBy(a => a.Action)
            .Select(g => new { Action = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Action, x => x.Count);
    }

    public async Task<List<DailyActivityCount>> GetDailyCountsAsync(int days = 30)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        var raw = await db.UserActivities
            .Where(a => a.Timestamp >= since)
            .GroupBy(a => a.Timestamp.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return raw.Select(x => new DailyActivityCount(DateOnly.FromDateTime(x.Date), x.Count)).ToList();
    }

    public async Task<int> GetActiveUserCountAsync(int days = 7)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return await db.UserActivities
            .Where(a => a.Timestamp >= since)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();
    }
}
