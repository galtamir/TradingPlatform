using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

/// <summary>
/// Shared fixture that provides a fully configured Identity stack backed by an in-memory database.
/// Each test class that uses this gets its own isolated database instance.
/// </summary>
public class IdentityTestFixture : IDisposable
{
    public ServiceProvider Services { get; }
    public UserManager<ApplicationUser> UserManager { get; }
    public RoleManager<IdentityRole> RoleManager { get; }
    public SignInManager<ApplicationUser> SignInManager { get; }
    public ApplicationDbContext DbContext { get; }

    public IdentityTestFixture()
    {
        var services = new ServiceCollection();

        // Unique DB per fixture instance
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase($"TestIdentityDb_{Guid.NewGuid()}"));

        services.AddLogging(b => b.AddDebug());

        services.AddIdentityCore<ApplicationUser>(options =>
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

        // Required by SignInManager
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        }).AddIdentityCookies();

        // HttpContextAccessor needed by SignInManager
        services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor,
            Microsoft.AspNetCore.Http.HttpContextAccessor>();

        Services = services.BuildServiceProvider();
        DbContext = Services.GetRequiredService<ApplicationDbContext>();
        DbContext.Database.EnsureCreated();

        UserManager = Services.GetRequiredService<UserManager<ApplicationUser>>();
        RoleManager = Services.GetRequiredService<RoleManager<IdentityRole>>();
        SignInManager = Services.GetRequiredService<SignInManager<ApplicationUser>>();
    }

    public async Task<ApplicationUser> CreateTestUserAsync(
        string email = "test@example.com",
        string password = "TestPass1234",
        string displayName = "Test User",
        string? role = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await UserManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        if (role is not null)
        {
            if (!await RoleManager.RoleExistsAsync(role))
                await RoleManager.CreateAsync(new IdentityRole(role));

            await UserManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    public void Dispose()
    {
        DbContext.Dispose();
        Services.Dispose();
        GC.SuppressFinalize(this);
    }
}
