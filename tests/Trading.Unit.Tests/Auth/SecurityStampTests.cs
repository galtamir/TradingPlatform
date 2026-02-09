using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class SecurityStampTests : IAsyncLifetime
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
    public async Task NewUser_HasSecurityStamp()
    {
        var user = await _fixture.CreateTestUserAsync("stamp@example.com", "Password1234");

        var stamp = await _fixture.UserManager.GetSecurityStampAsync(user);

        Assert.NotNull(stamp);
        Assert.NotEmpty(stamp);
    }

    [Fact]
    public async Task UpdateSecurityStamp_ChangesStamp()
    {
        var user = await _fixture.CreateTestUserAsync("stampchange@example.com", "Password1234");
        var originalStamp = await _fixture.UserManager.GetSecurityStampAsync(user);

        await _fixture.UserManager.UpdateSecurityStampAsync(user);
        var newStamp = await _fixture.UserManager.GetSecurityStampAsync(user);

        Assert.NotEqual(originalStamp, newStamp);
    }

    [Fact]
    public async Task ChangePassword_UpdatesSecurityStamp()
    {
        var user = await _fixture.CreateTestUserAsync("passchange@example.com", "OldPass1234");
        var originalStamp = await _fixture.UserManager.GetSecurityStampAsync(user);

        await _fixture.UserManager.ChangePasswordAsync(user, "OldPass1234", "NewPass5678");
        var newStamp = await _fixture.UserManager.GetSecurityStampAsync(user);

        Assert.NotEqual(originalStamp, newStamp);
    }
}
