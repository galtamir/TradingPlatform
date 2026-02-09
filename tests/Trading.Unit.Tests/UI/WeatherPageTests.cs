using Bunit;
using Trading.Web.Components.Pages;

namespace Trading.Unit.Tests.UI;

public class WeatherPageTests : BunitContext
{
    [Fact]
    public void Weather_InitiallyShowsLoading()
    {
        var cut = Render<Weather>();

        // Before data loads, should show loading message
        Assert.Contains("Loading...", cut.Markup);
    }

    [Fact]
    public void Weather_RendersHeading()
    {
        var cut = Render<Weather>();

        var h1 = cut.Find("h1");
        h1.TextContent.MarkupMatches("Weather");
    }

    [Fact]
    public async Task Weather_ShowsTableAfterLoading()
    {
        var cut = Render<Weather>();

        // Wait for the async data to load (component has 500ms delay)
        await Task.Delay(600);
        cut.Render();

        // Should now have a table with forecasts
        var table = cut.Find("table");
        Assert.NotNull(table);

        // Should have 5 forecast rows
        var rows = cut.FindAll("tbody tr");
        Assert.Equal(5, rows.Count);
    }

    [Fact]
    public async Task Weather_TableHasCorrectHeaders()
    {
        var cut = Render<Weather>();

        await Task.Delay(600);
        cut.Render();

        var headers = cut.FindAll("thead th");
        Assert.Equal(4, headers.Count);
        Assert.Equal("Date", headers[0].TextContent);
    }
}
