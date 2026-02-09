using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trading.Admin.Data;

namespace Trading.Admin.Components.Pages;

public partial class Dashboard
{
    [Inject] private UserManager<ApplicationUser> UserManager { get; set; } = default!;
    [Inject] private RoleManager<IdentityRole> RoleManager { get; set; } = default!;
    [Inject] private IUserActivityService ActivityService { get; set; } = default!;
    [Inject] private AdminDbContext Db { get; set; } = default!;
    [Inject] private ILogger<Dashboard> Logger { get; set; } = default!;

    protected string ActiveTab { get; set; } = "overview";
    protected string? StatusMessage { get; private set; }
    protected bool IsError { get; private set; }

    // KPI
    protected int TotalUsers { get; private set; }
    protected int ActiveUsersLast7Days { get; private set; }
    protected int TotalEvents30Days { get; private set; }
    protected int NewUsersLast30Days { get; private set; }

    // Overview
    protected List<DailyActivityCount>? DailyCounts { get; private set; }
    protected Dictionary<string, int>? ActionSummary { get; private set; }
    protected List<UserActivity>? RecentActivities { get; private set; }

    // Users
    protected List<UserRow>? AllUsers { get; private set; }
    protected string UserSearchTerm { get; set; } = "";
    protected bool ShowPasswordReset { get; set; }
    protected string? ResetUserId { get; set; }
    protected string? ResetEmail { get; set; }
    protected string? NewPassword { get; set; }
    protected string? ConfirmPassword { get; set; }

    // Flow
    protected string FlowUserFilter { get; set; } = "";
    protected string FlowActionFilter { get; set; } = "";
    protected List<UserActivity>? FlowActivities { get; private set; }
    protected Dictionary<string, int>? PageViewSummary { get; private set; }
    protected List<(string Email, int Count)>? UserActivityRanking { get; private set; }

    protected IEnumerable<UserRow>? FilteredUsers => AllUsers?
        .Where(u => string.IsNullOrEmpty(UserSearchTerm)
            || (u.Email?.Contains(UserSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
            || (u.DisplayName?.Contains(UserSearchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        await LoadKpiAsync();
        await LoadOverviewAsync();
        await LoadUsersAsync();
        await LoadFlowAsync();
    }

    protected void ClearStatus() => StatusMessage = null;

    private async Task LoadKpiAsync()
    {
        var allUsers = await UserManager.Users.ToListAsync();
        TotalUsers = allUsers.Count;
        NewUsersLast30Days = allUsers.Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        ActiveUsersLast7Days = await ActivityService.GetActiveUserCountAsync(7);
        TotalEvents30Days = (await ActivityService.GetActionSummaryAsync(30)).Values.Sum();
    }

    private async Task LoadOverviewAsync()
    {
        DailyCounts = await ActivityService.GetDailyCountsAsync(30);
        ActionSummary = await ActivityService.GetActionSummaryAsync(30);
        RecentActivities = await ActivityService.GetRecentAsync(25);
    }

    private async Task LoadUsersAsync()
    {
        var users = await UserManager.Users.ToListAsync();
        AllUsers = [];
        foreach (var user in users)
        {
            var roles = await UserManager.GetRolesAsync(user);
            var lastActivity = await Db.UserActivities
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => (DateTime?)a.Timestamp)
                .FirstOrDefaultAsync();

            AllUsers.Add(new UserRow
            {
                Id = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                Roles = [.. roles],
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd > DateTimeOffset.UtcNow,
                CreatedAt = user.CreatedAt,
                LastActive = lastActivity
            });
        }
    }

    private async Task LoadFlowAsync()
    {
        FlowActivities = string.IsNullOrEmpty(FlowUserFilter)
            ? await ActivityService.GetRecentAsync(100)
            : await ActivityService.GetByUserAsync(FlowUserFilter, 100);

        if (!string.IsNullOrEmpty(FlowActionFilter))
            FlowActivities = FlowActivities.Where(a => a.Action == FlowActionFilter).ToList();

        PageViewSummary = await Db.UserActivities
            .Where(a => a.Action == "PageView" && a.Timestamp >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(a => a.Detail ?? "Unknown")
            .Select(g => new { Page = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Page, x => x.Count);

        var ranking = await Db.UserActivities
            .Where(a => a.Timestamp >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(a => a.UserEmail)
            .Select(g => new { Email = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();
        UserActivityRanking = ranking.Select(r => (r.Email, r.Count)).ToList();
    }

    protected async Task ApplyFlowFiltersAsync() => await LoadFlowAsync();

    protected void ShowResetPassword(string userId, string email)
    {
        ResetUserId = userId;
        ResetEmail = email;
        NewPassword = null;
        ConfirmPassword = null;
        ShowPasswordReset = true;
    }

    protected void CloseResetPassword() => ShowPasswordReset = false;

    protected async Task ResetPasswordAsync()
    {
        if (string.IsNullOrEmpty(ResetUserId) || string.IsNullOrEmpty(NewPassword))
        {
            StatusMessage = "Password is required.";
            IsError = true;
            ShowPasswordReset = false;
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            StatusMessage = "Passwords do not match.";
            IsError = true;
            ShowPasswordReset = false;
            return;
        }

        var user = await UserManager.FindByIdAsync(ResetUserId);
        if (user is null) return;

        var token = await UserManager.GeneratePasswordResetTokenAsync(user);
        var result = await UserManager.ResetPasswordAsync(user, token, NewPassword);

        if (result.Succeeded)
        {
            StatusMessage = $"Password reset for '{user.Email}'.";
            IsError = false;
            Logger.LogInformation("Admin reset password for: {Email}", user.Email);
            await ActivityService.LogAsync(user.Id, user.Email ?? "", "PasswordReset", "Admin-initiated reset");
        }
        else
        {
            StatusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            IsError = true;
        }

        ShowPasswordReset = false;
        await LoadUsersAsync();
    }

    protected async Task ToggleAdminAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user is null) return;

        if (await UserManager.IsInRoleAsync(user, "Admin"))
            await UserManager.RemoveFromRoleAsync(user, "Admin");
        else
            await UserManager.AddToRoleAsync(user, "Admin");

        StatusMessage = $"Updated roles for '{user.Email}'.";
        IsError = false;
        await LoadUsersAsync();
    }

    protected async Task UnlockUserAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user is null) return;

        await UserManager.SetLockoutEndDateAsync(user, null);
        await UserManager.ResetAccessFailedCountAsync(user);
        StatusMessage = $"Unlocked '{user.Email}'.";
        IsError = false;
        await LoadUsersAsync();
    }

    protected async Task DeleteUserAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user is null) return;

        var result = await UserManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            StatusMessage = $"Deleted '{user.Email}'.";
            IsError = false;
            Logger.LogInformation("Admin deleted user: {Email}", user.Email);
        }
        else
        {
            StatusMessage = "Failed to delete user.";
            IsError = true;
        }

        await LoadKpiAsync();
        await LoadUsersAsync();
    }

    protected static string GetActionIcon(string action) => action switch
    {
        "Login" => "🔑",
        "Logout" => "🚪",
        "PageView" => "📄",
        "ProfileUpdate" => "✏️",
        "PasswordChange" or "PasswordReset" => "🔒",
        _ => "📌"
    };

    protected static string GetActionColorClass(string action) => action switch
    {
        "Login" => "flow-marker-login",
        "Logout" => "flow-marker-logout",
        "PageView" => "flow-marker-page",
        _ => "flow-marker-other"
    };

    protected sealed class UserRow
    {
        public string Id { get; init; } = "";
        public string Email { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public List<string> Roles { get; init; } = [];
        public bool EmailConfirmed { get; init; }
        public bool IsLockedOut { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? LastActive { get; init; }
    }
}
