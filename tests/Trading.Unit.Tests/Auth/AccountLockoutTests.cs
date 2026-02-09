using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class AccountLockoutTests : IAsyncLifetime
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
    public async Task NewUser_IsNotLockedOut()
    {
        var user = await _fixture.CreateTestUserAsync("notlocked@example.com", "Password1234");

        Assert.False(await _fixture.UserManager.IsLockedOutAsync(user));
    }

    [Fact]
    public async Task SetLockoutEnd_LocksOutUser()
    {
        var user = await _fixture.CreateTestUserAsync("lockme@example.com", "Password1234");
        await _fixture.UserManager.SetLockoutEnabledAsync(user, true);

        await _fixture.UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddHours(1));

        Assert.True(await _fixture.UserManager.IsLockedOutAsync(user));
    }

    [Fact]
    public async Task SetLockoutEnd_PastDate_UserNotLockedOut()
    {
        var user = await _fixture.CreateTestUserAsync("pastlock@example.com", "Password1234");
        await _fixture.UserManager.SetLockoutEnabledAsync(user, true);

        await _fixture.UserManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddHours(-1));

        Assert.False(await _fixture.UserManager.IsLockedOutAsync(user));
    }

    [Fact]
    public async Task AccessFailedCount_IncrementsOnFailure()
    {
        var user = await _fixture.CreateTestUserAsync("failcount@example.com", "Password1234");
        await _fixture.UserManager.SetLockoutEnabledAsync(user, true);

        await _fixture.UserManager.AccessFailedAsync(user);
        await _fixture.UserManager.AccessFailedAsync(user);

        var count = await _fixture.UserManager.GetAccessFailedCountAsync(user);
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ResetAccessFailedCount_ResetsToZero()
    {
        var user = await _fixture.CreateTestUserAsync("resetfail@example.com", "Password1234");
        await _fixture.UserManager.AccessFailedAsync(user);
        await _fixture.UserManager.AccessFailedAsync(user);

        await _fixture.UserManager.ResetAccessFailedCountAsync(user);

        var count = await _fixture.UserManager.GetAccessFailedCountAsync(user);
        Assert.Equal(0, count);
    }
}
