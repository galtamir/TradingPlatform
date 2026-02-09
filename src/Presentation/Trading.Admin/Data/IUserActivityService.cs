namespace Trading.Admin.Data;

public interface IUserActivityService
{
    Task LogAsync(string userId, string email, string action, string? detail = null);
    Task<List<UserActivity>> GetRecentAsync(int count = 50);
    Task<List<UserActivity>> GetByUserAsync(string userId, int count = 50);
    Task<Dictionary<string, int>> GetActionSummaryAsync(int days = 30);
    Task<List<DailyActivityCount>> GetDailyCountsAsync(int days = 30);
    Task<int> GetActiveUserCountAsync(int days = 7);
}

public record DailyActivityCount(DateOnly Date, int Count);
