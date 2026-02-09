using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Trading.Web.Components.Pages.Admin;

namespace Trading.Unit.Tests.Auth;

/// <summary>
/// Verifies that pages have the correct authorization attributes applied.
/// This ensures the [Authorize] attributes aren't accidentally removed.
/// </summary>
public class PageAuthorizationAttributeTests
{
    [Fact]
    public void AdminDataPage_HasAuthorizeAttribute()
    {
        var attr = typeof(Data).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
    }

    [Fact]
    public void AdminDataPage_RequiresAdminOnlyPolicy()
    {
        var attr = typeof(Data).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("AdminOnly", attr.Policy);
    }

    [Fact]
    public void HomePage_DoesNotRequireAuthorization()
    {
        var attr = typeof(Trading.Web.Components.Pages.Home)
            .GetCustomAttribute<AuthorizeAttribute>();
        Assert.Null(attr);
    }

    [Fact]
    public void WeatherPage_DoesNotRequireAuthorization()
    {
        var attr = typeof(Trading.Web.Components.Pages.Weather)
            .GetCustomAttribute<AuthorizeAttribute>();
        Assert.Null(attr);
    }
}
