using Bunit;
using Trading.Admin.Components.Pages;

namespace Trading.Unit.Tests.Admin;

public class AdminHomeTests : BunitContext
{
    [Fact]
    public void Home_RendersHeading()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetPolicies("AdminOnly");

        var cut = Render<Home>();

        var h1 = cut.Find("h1");
        Assert.Contains("Admin Portal", h1.TextContent);
    }

    [Fact]
    public void Home_ShowsDashboardCard()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetPolicies("AdminOnly");

        var cut = Render<Home>();

        Assert.Contains("Dashboard", cut.Markup);
        Assert.Contains("dashboard", cut.Markup);
    }

    [Fact]
    public void Home_ShowsDataManagementCard()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetPolicies("AdminOnly");

        var cut = Render<Home>();

        Assert.Contains("Data Management", cut.Markup);
        Assert.Contains("data", cut.Markup);
    }

    [Fact]
    public void Home_HasTwoCards()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetPolicies("AdminOnly");

        var cut = Render<Home>();

        var cards = cut.FindAll(".card");
        Assert.Equal(2, cards.Count);
    }

    [Fact]
    public void Home_HasNavigationButtons()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");
        authContext.SetPolicies("AdminOnly");

        var cut = Render<Home>();

        var buttons = cut.FindAll(".btn-primary");
        Assert.Equal(2, buttons.Count);
    }
}
