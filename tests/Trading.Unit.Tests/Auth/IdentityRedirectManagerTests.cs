using Trading.Web.Components.Account;

namespace Trading.Unit.Tests.Auth;

public class IdentityRedirectManagerTests
{
    [Fact]
    public void StatusCookieName_IsExpectedValue()
    {
        Assert.Equal("Identity.StatusMessage", IdentityRedirectManager.StatusCookieName);
    }
}
