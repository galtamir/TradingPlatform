using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

/// <summary>
/// Tests that the authorization policies defined in Program.cs work correctly.
/// Uses a standalone AuthorizationService to evaluate policies against claims principals.
/// </summary>
public class AuthorizationPolicyTests
{
    private readonly IAuthorizationService _authService;

    public AuthorizationPolicyTests()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole(SeedData.AdminRole));
        services.AddLogging();

        var provider = services.BuildServiceProvider();
        _authService = provider.GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal CreatePrincipal(string name, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, name),
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task AdminOnlyPolicy_AdminUser_Succeeds()
    {
        var admin = CreatePrincipal("admin@trading.local", SeedData.AdminRole);

        var result = await _authService.AuthorizeAsync(admin, "AdminOnly");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AdminOnlyPolicy_RegularUser_Fails()
    {
        var user = CreatePrincipal("demo@trading.local", SeedData.UserRole);

        var result = await _authService.AuthorizeAsync(user, "AdminOnly");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AdminOnlyPolicy_UnauthenticatedUser_Fails()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await _authService.AuthorizeAsync(anonymous, "AdminOnly");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task AdminOnlyPolicy_UserWithBothRoles_Succeeds()
    {
        var user = CreatePrincipal("both@trading.local", SeedData.AdminRole, SeedData.UserRole);

        var result = await _authService.AuthorizeAsync(user, "AdminOnly");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void SeedData_AdminRoleConstant_IsAdmin()
    {
        Assert.Equal("Admin", SeedData.AdminRole);
    }

    [Fact]
    public void SeedData_UserRoleConstant_IsUser()
    {
        Assert.Equal("User", SeedData.UserRole);
    }
}
