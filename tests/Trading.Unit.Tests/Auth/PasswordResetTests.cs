using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class PasswordResetTests : IAsyncLifetime
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
    public async Task GeneratePasswordResetToken_ReturnsValidToken()
    {
        var user = await _fixture.CreateTestUserAsync("reset@example.com", "OldPass1234");

        var token = await _fixture.UserManager.GeneratePasswordResetTokenAsync(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_Succeeds()
    {
        var user = await _fixture.CreateTestUserAsync("resetvalid@example.com", "OldPass1234");
        var token = await _fixture.UserManager.GeneratePasswordResetTokenAsync(user);

        var result = await _fixture.UserManager.ResetPasswordAsync(user, token, "NewPass5678");

        Assert.True(result.Succeeded);
        Assert.True(await _fixture.UserManager.CheckPasswordAsync(user, "NewPass5678"));
        Assert.False(await _fixture.UserManager.CheckPasswordAsync(user, "OldPass1234"));
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_Fails()
    {
        var user = await _fixture.CreateTestUserAsync("resetinvalid@example.com", "OldPass1234");

        var result = await _fixture.UserManager.ResetPasswordAsync(user, "invalid-token", "NewPass5678");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "InvalidToken");
    }

    [Fact]
    public async Task ChangePassword_WithCorrectOldPassword_Succeeds()
    {
        var user = await _fixture.CreateTestUserAsync("change@example.com", "OldPass1234");

        var result = await _fixture.UserManager.ChangePasswordAsync(user, "OldPass1234", "NewPass5678");

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ChangePassword_WithWrongOldPassword_Fails()
    {
        var user = await _fixture.CreateTestUserAsync("changewrong@example.com", "OldPass1234");

        var result = await _fixture.UserManager.ChangePasswordAsync(user, "WrongPass999", "NewPass5678");

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ChangePassword_NewPasswordTooShort_Fails()
    {
        var user = await _fixture.CreateTestUserAsync("changeshort@example.com", "OldPass1234");

        var result = await _fixture.UserManager.ChangePasswordAsync(user, "OldPass1234", "Ab1");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "PasswordTooShort");
    }
}
