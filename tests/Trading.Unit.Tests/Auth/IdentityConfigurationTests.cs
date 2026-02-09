using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class IdentityConfigurationTests : IClassFixture<IdentityTestFixture>
{
    private readonly IdentityTestFixture _fixture;

    public IdentityConfigurationTests(IdentityTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void PasswordPolicy_RequiresMinimumLength8()
    {
        var options = _fixture.Services.GetRequiredService<IOptions<IdentityOptions>>();
        Assert.Equal(8, options.Value.Password.RequiredLength);
    }

    [Fact]
    public void PasswordPolicy_RequiresDigit()
    {
        var options = _fixture.Services.GetRequiredService<IOptions<IdentityOptions>>();
        Assert.True(options.Value.Password.RequireDigit);
    }

    [Fact]
    public void PasswordPolicy_DoesNotRequireNonAlphanumeric()
    {
        var options = _fixture.Services.GetRequiredService<IOptions<IdentityOptions>>();
        Assert.False(options.Value.Password.RequireNonAlphanumeric);
    }

    [Fact]
    public void SignIn_DoesNotRequireConfirmedAccount()
    {
        var options = _fixture.Services.GetRequiredService<IOptions<IdentityOptions>>();
        Assert.False(options.Value.SignIn.RequireConfirmedAccount);
    }

    [Fact]
    public void IdentitySchemaVersion_IsVersion3()
    {
        var options = _fixture.Services.GetRequiredService<IOptions<IdentityOptions>>();
        Assert.Equal(IdentitySchemaVersions.Version3, options.Value.Stores.SchemaVersion);
    }

    [Fact]
    public void UserManager_IsRegistered()
    {
        var userManager = _fixture.Services.GetService<UserManager<ApplicationUser>>();
        Assert.NotNull(userManager);
    }

    [Fact]
    public void RoleManager_IsRegistered()
    {
        var roleManager = _fixture.Services.GetService<RoleManager<IdentityRole>>();
        Assert.NotNull(roleManager);
    }

    [Fact]
    public void SignInManager_IsRegistered()
    {
        var signInManager = _fixture.Services.GetService<SignInManager<ApplicationUser>>();
        Assert.NotNull(signInManager);
    }

    [Fact]
    public void UserManager_SupportsUserRole()
    {
        Assert.True(_fixture.UserManager.SupportsUserRole);
    }

    [Fact]
    public void UserManager_SupportsUserEmail()
    {
        Assert.True(_fixture.UserManager.SupportsUserEmail);
    }

    [Fact]
    public void UserManager_SupportsUserSecurityStamp()
    {
        Assert.True(_fixture.UserManager.SupportsUserSecurityStamp);
    }
}
