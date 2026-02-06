using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trading.Web.Data;

namespace Trading.Web.Components.Pages.Admin;

public partial class Data
{
    [Inject]
    private UserManager<ApplicationUser> UserManager { get; set; } = default!;

    [Inject]
    private RoleManager<IdentityRole> RoleManager { get; set; } = default!;

    [Inject]
    private ILogger<Data> Logger { get; set; } = default!;

    // State
    protected string ActiveTab { get; set; } = "users";
    protected string? StatusMessage { get; private set; }
    protected bool IsError { get; private set; }

    // Stats
    protected int UserCount { get; private set; }
    protected int RoleCount { get; private set; }
    protected int ConfirmedCount { get; private set; }
    protected int LockedOutCount { get; private set; }

    // Data
    protected List<UserViewModel>? Users { get; private set; }
    protected List<RoleViewModel>? Roles { get; private set; }

    // Form models
    protected NewUserModel NewUser { get; set; } = new();
    protected NewRoleModel NewRole { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    protected void ClearStatus() => StatusMessage = null;

    private async Task LoadDataAsync()
    {
        var allUsers = await UserManager.Users.ToListAsync();
        UserCount = allUsers.Count;
        ConfirmedCount = allUsers.Count(u => u.EmailConfirmed);
        LockedOutCount = allUsers.Count(u => u.LockoutEnd > DateTimeOffset.UtcNow);

        Users = [];
        foreach (var user in allUsers)
        {
            var roles = await UserManager.GetRolesAsync(user);
            Users.Add(new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                Roles = [.. roles],
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt
            });
        }

        var allRoles = await RoleManager.Roles.ToListAsync();
        RoleCount = allRoles.Count;
        Roles = [];
        foreach (var role in allRoles)
        {
            var usersInRole = await UserManager.GetUsersInRoleAsync(role.Name!);
            Roles.Add(new RoleViewModel { Name = role.Name!, UserCount = usersInRole.Count });
        }
    }

    protected async Task CreateUserAsync()
    {
        var user = new ApplicationUser
        {
            UserName = NewUser.Email,
            Email = NewUser.Email,
            DisplayName = NewUser.DisplayName ?? NewUser.Email ?? "",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await UserManager.CreateAsync(user, NewUser.Password ?? "");
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(NewUser.Role))
                await UserManager.AddToRoleAsync(user, NewUser.Role);

            StatusMessage = $"User '{NewUser.Email}' created successfully.";
            IsError = false;
            NewUser = new();
            Logger.LogInformation("Admin created user: {Email}", user.Email);
        }
        else
        {
            StatusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            IsError = true;
        }

        await LoadDataAsync();
    }

    protected async Task CreateRoleAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRole.Name))
            return;

        var roleName = NewRole.Name.Trim();
        var result = await RoleManager.CreateAsync(new IdentityRole(roleName));
        if (result.Succeeded)
        {
            StatusMessage = $"Role '{roleName}' created.";
            IsError = false;
            NewRole = new();
            Logger.LogInformation("Admin created role: {Role}", roleName);
        }
        else
        {
            StatusMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            IsError = true;
        }

        await LoadDataAsync();
    }

    protected async Task DeleteUserAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user is null) return;

        var result = await UserManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            StatusMessage = $"User '{user.Email}' deleted.";
            IsError = false;
            Logger.LogInformation("Admin deleted user: {Email}", user.Email);
        }
        else
        {
            StatusMessage = "Failed to delete user.";
            IsError = true;
        }

        await LoadDataAsync();
    }

    protected async Task PromoteToAdminAsync(string userId)
    {
        var user = await UserManager.FindByIdAsync(userId);
        if (user is null) return;

        if (await UserManager.IsInRoleAsync(user, SeedData.AdminRole))
            await UserManager.RemoveFromRoleAsync(user, SeedData.AdminRole);
        else
            await UserManager.AddToRoleAsync(user, SeedData.AdminRole);

        StatusMessage = $"Updated roles for '{user.Email}'.";
        IsError = false;
        await LoadDataAsync();
    }

    // View Models
    protected sealed class UserViewModel
    {
        public string Id { get; init; } = "";
        public string Email { get; init; } = "";
        public string DisplayName { get; init; } = "";
        public List<string> Roles { get; init; } = [];
        public bool EmailConfirmed { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    protected sealed class RoleViewModel
    {
        public string Name { get; init; } = "";
        public int UserCount { get; init; }
    }

    protected sealed class NewUserModel
    {
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    protected sealed class NewRoleModel
    {
        public string? Name { get; set; }
    }
}
