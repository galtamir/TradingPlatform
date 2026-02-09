using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Trading.Web.Components.Layout;
using Trading.Web.Data;

namespace Trading.Unit.Tests.UI;

public class NavMenuTests : BunitContext
{
    [Fact]
    public void NavMenu_RendersNavBrand()
    {
        AddAuthorization();

        var cut = Render<NavMenu>();

        var brand = cut.Find(".navbar-brand");
        Assert.Equal("Trading Platform", brand.TextContent);
    }

    [Fact]
    public void NavMenu_ShowsPublicLinks()
    {
        AddAuthorization();

        var cut = Render<NavMenu>();

        // Home, Counter, Weather are always visible
        var navLinks = cut.FindAll(".nav-link");
        Assert.True(navLinks.Count >= 3);

        Assert.Contains(navLinks, l => l.TextContent.Contains("Home"));
        Assert.Contains(navLinks, l => l.TextContent.Contains("Counter"));
        Assert.Contains(navLinks, l => l.TextContent.Contains("Weather"));
    }

    [Fact]
    public void NavMenu_Unauthenticated_HidesAuthRequiredLink()
    {
        AddAuthorization(); // Default is unauthenticated

        var cut = Render<NavMenu>();

        Assert.DoesNotContain(cut.Markup, "Auth Required");
    }

    [Fact]
    public void NavMenu_Unauthenticated_HidesAdminDataLink()
    {
        AddAuthorization();

        var cut = Render<NavMenu>();

        Assert.DoesNotContain(cut.Markup, "Data");
        Assert.DoesNotContain(cut.Markup, "admin/data");
    }

    [Fact]
    public void NavMenu_Authenticated_ShowsAuthRequiredLink()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");
        // Need to set authorized state explicitly
        authContext.SetRoles("User");

        var cut = Render<NavMenu>();

        // The AuthorizeView should show the Authorized content
        Assert.Contains("Auth Required", cut.Markup);
    }

    [Fact]
    public void NavMenu_AuthenticatedNonAdmin_HidesAdminDataLink()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("demo@trading.local");
        authContext.SetRoles(SeedData.UserRole);
        // Don't set AdminOnly policy - user is not admin

        var cut = Render<NavMenu>();

        // Should not show admin data link
        Assert.DoesNotContain("admin/data", cut.Markup);
    }

    [Fact]
    public void NavMenu_Admin_ShowsAdminDataLink()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetRoles(SeedData.AdminRole);
        authContext.SetPolicies("AdminOnly");

        var cut = Render<NavMenu>();

        // Admin should see the data link
        Assert.Contains("admin/data", cut.Markup);
        Assert.Contains("Data", cut.Markup);
    }
}
