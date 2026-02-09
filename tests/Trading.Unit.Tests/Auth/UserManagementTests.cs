using Microsoft.AspNetCore.Identity;
using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class UserManagementTests : IAsyncLifetime
{
    private IdentityTestFixture _fixture = null!;

    public Task InitializeAsync()
    {
        _fixture = new IdentityTestFixture();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _fixture.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateUser_WithValidData_Succeeds()
    {
        var user = new ApplicationUser
        {
            UserName = "newuser@example.com",
            Email = "newuser@example.com",
            DisplayName = "New User",
            EmailConfirmed = true
        };

        var result = await _fixture.UserManager.CreateAsync(user, "ValidPass1234");

        Assert.True(result.Succeeded);
        Assert.NotNull(await _fixture.UserManager.FindByEmailAsync("newuser@example.com"));
    }

    [Fact]
    public async Task CreateUser_WithShortPassword_Fails()
    {
        var user = new ApplicationUser
        {
            UserName = "short@example.com",
            Email = "short@example.com",
            DisplayName = "Short Pass"
        };

        var result = await _fixture.UserManager.CreateAsync(user, "Ab1"); // Too short

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "PasswordTooShort");
    }

    [Fact]
    public async Task CreateUser_WithNoDigitPassword_Fails()
    {
        var user = new ApplicationUser
        {
            UserName = "nodigit@example.com",
            Email = "nodigit@example.com",
            DisplayName = "No Digit"
        };

        var result = await _fixture.UserManager.CreateAsync(user, "NoDigitPassword");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "PasswordRequiresDigit");
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_Fails()
    {
        await _fixture.CreateTestUserAsync("dup@example.com", "Password1234");

        var duplicate = new ApplicationUser
        {
            UserName = "dup@example.com",
            Email = "dup@example.com",
            DisplayName = "Duplicate"
        };

        var result = await _fixture.UserManager.CreateAsync(duplicate, "Password1234");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "DuplicateUserName");
    }

    [Fact]
    public async Task FindByEmail_ExistingUser_ReturnsUser()
    {
        await _fixture.CreateTestUserAsync("findme@example.com", "Password1234", "Find Me");

        var found = await _fixture.UserManager.FindByEmailAsync("findme@example.com");

        Assert.NotNull(found);
        Assert.Equal("Find Me", found.DisplayName);
    }

    [Fact]
    public async Task FindByEmail_NonExistentUser_ReturnsNull()
    {
        var found = await _fixture.UserManager.FindByEmailAsync("ghost@example.com");
        Assert.Null(found);
    }

    [Fact]
    public async Task DeleteUser_ExistingUser_Succeeds()
    {
        var user = await _fixture.CreateTestUserAsync("deleteme@example.com", "Password1234");

        var result = await _fixture.UserManager.DeleteAsync(user);

        Assert.True(result.Succeeded);
        Assert.Null(await _fixture.UserManager.FindByEmailAsync("deleteme@example.com"));
    }

    [Fact]
    public async Task CheckPassword_CorrectPassword_ReturnsTrue()
    {
        var user = await _fixture.CreateTestUserAsync("checkpass@example.com", "CorrectPass1");

        var isValid = await _fixture.UserManager.CheckPasswordAsync(user, "CorrectPass1");

        Assert.True(isValid);
    }

    [Fact]
    public async Task CheckPassword_WrongPassword_ReturnsFalse()
    {
        var user = await _fixture.CreateTestUserAsync("wrongpass@example.com", "CorrectPass1");

        var isValid = await _fixture.UserManager.CheckPasswordAsync(user, "WrongPass999");

        Assert.False(isValid);
    }

    [Fact]
    public async Task GenerateEmailConfirmationToken_ReturnsToken()
    {
        var user = await _fixture.CreateTestUserAsync("token@example.com", "Password1234");

        var token = await _fixture.UserManager.GenerateEmailConfirmationTokenAsync(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_Succeeds()
    {
        var user = new ApplicationUser
        {
            UserName = "confirm@example.com",
            Email = "confirm@example.com",
            DisplayName = "Confirm Me",
            EmailConfirmed = false
        };
        await _fixture.UserManager.CreateAsync(user, "Password1234");

        var token = await _fixture.UserManager.GenerateEmailConfirmationTokenAsync(user);
        var result = await _fixture.UserManager.ConfirmEmailAsync(user, token);

        Assert.True(result.Succeeded);
        Assert.True(await _fixture.UserManager.IsEmailConfirmedAsync(user));
    }
}
