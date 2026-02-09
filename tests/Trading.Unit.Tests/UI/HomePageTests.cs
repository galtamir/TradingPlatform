using Bunit;
using Trading.Web.Components.Pages;

namespace Trading.Unit.Tests.UI;

public class HomePageTests : BunitContext
{
    [Fact]
    public void Home_RendersHeading()
    {
        var cut = Render<Home>();

        var h1 = cut.Find("h1");
        h1.TextContent.MarkupMatches("Hello, world!");
    }

    [Fact]
    public void Home_RendersWelcomeText()
    {
        var cut = Render<Home>();

        Assert.Contains("Welcome to your new app", cut.Markup);
    }
}
