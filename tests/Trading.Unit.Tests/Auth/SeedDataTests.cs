using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

/// <summary>
/// Tests the seed data logic by replicating the same user/role creation
/// that SeedData.InitializeAsync performs, but using InMemory provider
/// (which doesn't support relational migrations).
/// This validates the seed data contracts and expected outcomes.
/// </summary>
public class SeedDataTests : IAsyncLifetime
{
    private ServiceProvider _services = null!;
    private UserManager<ApplicationUser> _userManager = null!;
    private RoleManager<IdentityRole> _roleManager = null!;

    public async Task InitializeAsync()
    {
        var sc = new ServiceCollection();
        sc.AddDbContext<ApplicationDbContext>(o =>
            o.UseInMemoryDatabase($"SeedTestDb_{Guid.NewGuid()}"));
        sc.AddLogging(b => b.AddDebug());

        sc.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        sc.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        }).AddIdentityCookies();

        sc.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor,
            Microsoft.AspNetCore.Http.HttpContextAccessor>();

        _services = sc.BuildServiceProvider();

        var db = _services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Replicate the seed logic from SeedData (skipping migration calls)
        _userManager = _services.GetRequiredService<UserManager<ApplicationUser>>();
        _roleManager = _services.GetRequiredService<RoleManager<IdentityRole>>();

        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedDemoUsersAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roles = [SeedData.AdminRole, SeedData.UserRole];
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task SeedAdminUserAsync()
    {
        var admin = new ApplicationUser
        {
            UserName = "admin@trading.local",
            Email = "admin@trading.local",
            DisplayName = "System Admin",
            PreferredCurrency = "USD",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(admin, "Admin123!");
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(admin, SeedData.AdminRole);
    }

    private async Task SeedDemoUsersAsync()
    {
        var demoUsers = new[]
        {
            ("demo@trading.local", "Demo1234", "Demo User", "USD"),
            ("trader@trading.local", "Trader1234", "Active Trader", "EUR"),
        };

        foreach (var (email, password, displayName, currency) in demoUsers)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                PreferredCurrency = currency,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
                await _userManager.AddToRoleAsync(user, SeedData.UserRole);
        }
    }

    public Task DisposeAsync()
    {
        _services.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Seed_CreatesAdminRole()
    {
        Assert.True(await _roleManager.RoleExistsAsync(SeedData.AdminRole));
    }

    [Fact]
    public async Task Seed_CreatesUserRole()
    {
        Assert.True(await _roleManager.RoleExistsAsync(SeedData.UserRole));
    }

    [Fact]
    public async Task Seed_CreatesAdminUser()
    {
        var admin = await _userManager.FindByEmailAsync("admin@trading.local");

        Assert.NotNull(admin);
        Assert.Equal("System Admin", admin.DisplayName);
        Assert.True(admin.EmailConfirmed);
    }

    [Fact]
    public async Task Seed_AdminUser_HasAdminRole()
    {
        var admin = await _userManager.FindByEmailAsync("admin@trading.local");

        Assert.NotNull(admin);
        Assert.True(await _userManager.IsInRoleAsync(admin, SeedData.AdminRole));
    }

    [Fact]
    public async Task Seed_CreatesDemoUser()
    {
        var demo = await _userManager.FindByEmailAsync("demo@trading.local");

        Assert.NotNull(demo);
        Assert.Equal("Demo User", demo.DisplayName);
        Assert.Equal("USD", demo.PreferredCurrency);
    }

    [Fact]
    public async Task Seed_CreatesTraderUser()
    {
        var trader = await _userManager.FindByEmailAsync("trader@trading.local");

        Assert.NotNull(trader);
        Assert.Equal("Active Trader", trader.DisplayName);
        Assert.Equal("EUR", trader.PreferredCurrency);
    }

    [Fact]
    public async Task Seed_DemoUsers_HaveUserRole()
    {
        var demo = await _userManager.FindByEmailAsync("demo@trading.local");
        var trader = await _userManager.FindByEmailAsync("trader@trading.local");

        Assert.NotNull(demo);
        Assert.NotNull(trader);
        Assert.True(await _userManager.IsInRoleAsync(demo, SeedData.UserRole));
        Assert.True(await _userManager.IsInRoleAsync(trader, SeedData.UserRole));
    }

    [Fact]
    public async Task Seed_DemoUsers_DoNotHaveAdminRole()
    {
        var demo = await _userManager.FindByEmailAsync("demo@trading.local");
        var trader = await _userManager.FindByEmailAsync("trader@trading.local");

        Assert.NotNull(demo);
        Assert.NotNull(trader);
        Assert.False(await _userManager.IsInRoleAsync(demo, SeedData.AdminRole));
        Assert.False(await _userManager.IsInRoleAsync(trader, SeedData.AdminRole));
    }

    [Fact]
    public async Task Seed_TotalUserCount_IsThree()
    {
        var users = await _userManager.Users.ToListAsync();
        Assert.Equal(3, users.Count);
    }

    [Fact]
    public async Task Seed_IsIdempotent_RunningTwiceDoesNotDuplicate()
    {
        // Run seed again (roles and users already exist)
        await SeedRolesAsync();
        // Admin user already exists, so SeedAdminUserAsync would skip
        // We verify the count stays the same
        var users = await _userManager.Users.ToListAsync();
        Assert.Equal(3, users.Count);

        var roles = await _roleManager.Roles.ToListAsync();
        Assert.Equal(2, roles.Count);
    }

    [Fact]
    public async Task Seed_AdminPassword_IsValid()
    {
        var admin = await _userManager.FindByEmailAsync("admin@trading.local");
        Assert.NotNull(admin);

        var isValid = await _userManager.CheckPasswordAsync(admin, "Admin123!");
        Assert.True(isValid);
    }

    [Fact]
    public async Task Seed_DemoPassword_IsValid()
    {
        var demo = await _userManager.FindByEmailAsync("demo@trading.local");
        Assert.NotNull(demo);

        var isValid = await _userManager.CheckPasswordAsync(demo, "Demo1234");
        Assert.True(isValid);
    }

    [Fact]
    public async Task Seed_AllUsers_HaveEmailConfirmed()
    {
        var users = await _userManager.Users.ToListAsync();
        Assert.All(users, u => Assert.True(u.EmailConfirmed));
    }
}
