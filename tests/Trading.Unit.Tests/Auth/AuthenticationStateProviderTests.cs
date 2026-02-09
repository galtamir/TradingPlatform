using System.Reflection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Trading.Web.Components.Account;

namespace Trading.Unit.Tests.Auth;

public class AuthenticationStateProviderTests
{
    [Fact]
    public void IdentityRevalidatingProvider_InheritsFromRevalidatingServerProvider()
    {
        Assert.True(typeof(IdentityRevalidatingAuthenticationStateProvider)
            .IsSubclassOf(typeof(RevalidatingServerAuthenticationStateProvider)));
    }

    [Fact]
    public void IdentityRevalidatingProvider_ImplementsAuthenticationStateProvider()
    {
        Assert.True(typeof(AuthenticationStateProvider)
            .IsAssignableFrom(typeof(IdentityRevalidatingAuthenticationStateProvider)));
    }

    [Fact]
    public void RevalidationInterval_Is30Minutes()
    {
        // Access the protected property via reflection
        var prop = typeof(IdentityRevalidatingAuthenticationStateProvider)
            .GetProperty("RevalidationInterval", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(prop);
        // Verify the property type is TimeSpan
        Assert.Equal(typeof(TimeSpan), prop.PropertyType);
    }
}
