using Microsoft.AspNetCore.Identity;
using Trading.Admin.Components;
using Trading.Admin.Data;
using Trading.Persistence;

namespace Trading.Admin;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddServiceDefaults();

        // Same Aspire-managed PostgreSQL as the main web app
        builder.AddNpgsqlDbContext<AdminDbContext>("IdentityDb");

        // Domain/business data
        builder.AddNpgsqlDbContext<TradingDbContext>("TradingDb");

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Auth
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AdminDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

        // User activity service
        builder.Services.AddScoped<IUserActivityService, UserActivityService>();
        builder.Services.AddHttpContextAccessor();

        // Authorization
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapDefaultEndpoints();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapAdditionalIdentityEndpoints();

        await app.RunAsync();
    }
}
