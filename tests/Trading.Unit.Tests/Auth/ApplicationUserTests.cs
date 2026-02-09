using Trading.Web.Data;

namespace Trading.Unit.Tests.Auth;

public class ApplicationUserTests
{
    [Fact]
    public void NewUser_HasDefaultValues()
    {
        var user = new ApplicationUser();

        Assert.Equal(string.Empty, user.DisplayName);
        Assert.Equal("USD", user.PreferredCurrency);
        Assert.True(user.CreatedAt <= DateTime.UtcNow);
        Assert.True(user.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public void User_CanSetProperties()
    {
        var now = DateTime.UtcNow;
        var user = new ApplicationUser
        {
            UserName = "trader@example.com",
            Email = "trader@example.com",
            DisplayName = "Active Trader",
            PreferredCurrency = "EUR",
            CreatedAt = now
        };

        Assert.Equal("trader@example.com", user.UserName);
        Assert.Equal("trader@example.com", user.Email);
        Assert.Equal("Active Trader", user.DisplayName);
        Assert.Equal("EUR", user.PreferredCurrency);
        Assert.Equal(now, user.CreatedAt);
    }

    [Fact]
    public void User_InheritsFromIdentityUser()
    {
        var user = new ApplicationUser();
        Assert.IsAssignableFrom<Microsoft.AspNetCore.Identity.IdentityUser>(user);
    }
}
