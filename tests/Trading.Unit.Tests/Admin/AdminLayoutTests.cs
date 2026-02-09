using Bunit;
using Trading.Admin.Components.Layout;

namespace Trading.Unit.Tests.Admin;

public class AdminLayoutTests : BunitContext
{
    [Fact]
    public void AdminLayout_RendersPageStructure()
    {
        AddAuthorization();

        var cut = Render<AdminLayout>();

        Assert.NotNull(cut.Find(".page"));
        Assert.NotNull(cut.Find(".sidebar"));
        Assert.NotNull(cut.Find("main"));
    }

    [Fact]
    public void AdminLayout_RendersSidebar()
    {
        AddAuthorization();

        var cut = Render<AdminLayout>();

        var sidebar = cut.Find(".sidebar");
        Assert.NotNull(sidebar);
    }

    [Fact]
    public void AdminLayout_RendersTopRow()
    {
        AddAuthorization();

        var cut = Render<AdminLayout>();

        var topRow = cut.Find(".top-row");
        Assert.NotNull(topRow);
    }

    [Fact]
    public void AdminLayout_RendersContentArea()
    {
        AddAuthorization();

        var cut = Render<AdminLayout>();

        var content = cut.Find(".content");
        Assert.NotNull(content);
    }

    [Fact]
    public void AdminLayout_Unauthenticated_ShowsLoginLink()
    {
        AddAuthorization();

        var cut = Render<AdminLayout>();

        Assert.Contains("Login", cut.Markup);
    }

    [Fact]
    public void AdminLayout_Authenticated_ShowsLogoutButton()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");

        var cut = Render<AdminLayout>();

        Assert.Contains("Logout", cut.Markup);
    }

    [Fact]
    public void AdminLayout_Authenticated_ShowsUserName()
    {
        var authContext = AddAuthorization();
        authContext.SetAuthorized("admin@trading.local");

        var cut = Render<AdminLayout>();

        Assert.Contains("admin@trading.local", cut.Markup);
    }

    [Fact]
    public void AdminLayout_HasErrorUI()
    {
        AddAuthorization();

        var cut = Render<AdminLayout>();

        var errorUi = cut.Find("#blazor-error-ui");
        Assert.NotNull(errorUi);
    }
}
