using Bunit;
using Trading.Admin.Components.Layout;

namespace Trading.Unit.Tests.Admin;

public class AdminNavTests : BunitContext
{
    [Fact]
    public void AdminNav_RendersNavBrand()
    {
        AddAuthorization();

        var cut = Render<AdminNav>();

        var brand = cut.Find(".navbar-brand");
        Assert.Contains("Admin Portal", brand.TextContent);
    }

    [Fact]
    public void AdminNav_ShowsHomeLink()
    {
        AddAuthorization();

        var cut = Render<AdminNav>();

        var navLinks = cut.FindAll(".nav-link");
        Assert.Contains(navLinks, l => l.TextContent.Contains("Home"));
    }

    [Fact]
    public void AdminNav_Unauthenticated_HidesDashboardLink()
    {
        AddAuthorization();

        var cut = Render<AdminNav>();

        Assert.DoesNotContain("Dashboard", cut.Markup);
    }

    [Fact]
    public void AdminNav_Unauthenticated_HidesDataLink()
    {
        AddAuthorization();

        var cut = Render<AdminNav>();

        Assert.DoesNotContain("data", cut.Markup.ToLowerInvariant().Replace("data-", ""));
    }

    [Fact]
    public void AdminNav_Admin_ShowsDashboardLink()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetRoles("Admin");
        authContext.SetPolicies("AdminOnly");

        var cut = Render<AdminNav>();

        Assert.Contains("Dashboard", cut.Markup);
        Assert.Contains("dashboard", cut.Markup);
    }

    [Fact]
    public void AdminNav_Admin_ShowsDataLink()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetRoles("Admin");
        authContext.SetPolicies("AdminOnly");

        var cut = Render<AdminNav>();

        var navLinks = cut.FindAll(".nav-link");
        Assert.Contains(navLinks, l => l.TextContent.Contains("Data"));
    }

    [Fact]
    public void AdminNav_HasNavigationToggler()
    {
        AddAuthorization();

        var cut = Render<AdminNav>();

        var toggler = cut.Find(".navbar-toggler");
        Assert.NotNull(toggler);
    }

    [Fact]
    public void AdminNav_HasNavScrollableSection()
    {
        AddAuthorization();

        var cut = Render<AdminNav>();

        var scrollable = cut.Find(".nav-scrollable");
        Assert.NotNull(scrollable);
    }
}
