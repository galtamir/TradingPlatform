using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Trading.Web.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var db = sp.GetRequiredService<ApplicationDbContext>();
        var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("Initializing database...");

        // Check if we have pending migrations
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();

        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
            await db.Database.MigrateAsync();
        }
        else if (!appliedMigrations.Any())
        {
            // No migrations exist at all — use EnsureCreated for dev
            logger.LogInformation("No migrations found, using EnsureCreated...");
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            logger.LogInformation("Database is up to date");
        }

        await SeedRolesAsync(sp, logger);
        await SeedAdminUserAsync(sp, logger);
        await SeedDemoUsersAsync(sp, logger);
        await SeedActivityDataAsync(sp, logger);

        logger.LogInformation("Database initialization complete");
    }

    private static async Task SeedRolesAsync(IServiceProvider sp, ILogger logger)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = [AdminRole, UserRole];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider sp, ILogger logger)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminEmail = "admin@trading.local";
        const string adminPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return;

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            DisplayName = "System Admin",
            PreferredCurrency = "USD",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
            logger.LogInformation("Seeded admin user: {Email}", adminEmail);
        }
        else
        {
            logger.LogError("Failed to seed admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedDemoUsersAsync(IServiceProvider sp, ILogger logger)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

        var demoUsers = new[]
        {
            ("demo@trading.local", "Demo1234", "Demo User", "USD"),
            ("trader@trading.local", "Trader1234", "Active Trader", "EUR"),
        };

        foreach (var (email, password, displayName, currency) in demoUsers)
        {
            if (await userManager.FindByEmailAsync(email) is not null)
                continue;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                DisplayName = displayName,
                PreferredCurrency = currency,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, UserRole);
                logger.LogInformation("Seeded demo user: {Email}", email);
            }
        }
    }

    private static async Task SeedActivityDataAsync(IServiceProvider sp, ILogger logger)
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        if (await db.UserActivities.AnyAsync())
            return;

        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var users = await userManager.Users.ToListAsync();
        if (users.Count == 0) return;

        var actions = new[]
        {
            ("Login", "Password sign-in"),
            ("Login", "Modal sign-in"),
            ("Logout", (string?)null),
            ("PageView", "/"),
            ("PageView", "/weather"),
            ("PageView", "/admin/data"),
            ("ProfileUpdate", "Changed display name"),
            ("PasswordChange", "Password updated"),
            ("PageView", "/Account/Manage"),
        };

        var random = new Random(42);
        var activities = new List<UserActivity>();

        for (var day = 30; day >= 0; day--)
        {
            var eventsToday = random.Next(3, 12);
            for (var i = 0; i < eventsToday; i++)
            {
                var user = users[random.Next(users.Count)];
                var (action, detail) = actions[random.Next(actions.Length)];
                activities.Add(new UserActivity
                {
                    UserId = user.Id,
                    UserEmail = user.Email ?? "",
                    Action = action,
                    Detail = detail,
                    IpAddress = $"192.168.1.{random.Next(1, 255)}",
                    Timestamp = DateTime.UtcNow.AddDays(-day).AddHours(random.Next(8, 22)).AddMinutes(random.Next(60))
                });
            }
        }

        db.UserActivities.AddRange(activities);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} activity records", activities.Count);
    }
}
