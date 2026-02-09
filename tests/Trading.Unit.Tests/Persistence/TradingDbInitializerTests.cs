using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Trading.Persistence;

namespace Trading.Unit.Tests.Persistence;

public class TradingDbInitializerTests
{
    private static ServiceProvider BuildServices(string dbName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<TradingDbContext>(options =>
            options.UseInMemoryDatabase(dbName));
        services.AddLogging(b => b.AddDebug());
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task InitializeAsync_CreatesDatabase()
    {
        var dbName = $"InitTest_{Guid.NewGuid()}";
        using var sp = BuildServices(dbName);

        await TradingDbInitializer.InitializeAsync(sp);

        // Verify we can connect via a new scope
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        Assert.True(await db.Database.CanConnectAsync());
    }

    [Fact]
    public async Task InitializeAsync_SeedsAssets()
    {
        var dbName = $"InitTest_{Guid.NewGuid()}";
        using var sp = BuildServices(dbName);

        await TradingDbInitializer.InitializeAsync(sp);

        // The initializer uses its own scope, so data should be visible in a new scope
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        var count = await db.Assets.CountAsync();

        Assert.Equal(10, count);
    }

    [Fact]
    public async Task InitializeAsync_SeedsExpectedSymbols()
    {
        var dbName = $"InitTest_{Guid.NewGuid()}";
        using var sp = BuildServices(dbName);

        await TradingDbInitializer.InitializeAsync(sp);

        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        var symbols = await db.Assets.Select(a => a.Symbol).ToListAsync();

        Assert.Contains("AAPL", symbols);
        Assert.Contains("MSFT", symbols);
        Assert.Contains("BTC", symbols);
        Assert.Contains("ETH", symbols);
        Assert.Contains("SPY", symbols);
    }

    [Fact]
    public async Task InitializeAsync_SeedsMultipleAssetTypes()
    {
        var dbName = $"InitTest_{Guid.NewGuid()}";
        using var sp = BuildServices(dbName);

        await TradingDbInitializer.InitializeAsync(sp);

        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        var types = await db.Assets.Select(a => a.AssetType).Distinct().ToListAsync();

        Assert.Contains("Stock", types);
        Assert.Contains("Crypto", types);
        Assert.Contains("ETF", types);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent()
    {
        var dbName = $"InitTest_{Guid.NewGuid()}";
        using var sp = BuildServices(dbName);

        await TradingDbInitializer.InitializeAsync(sp);
        await TradingDbInitializer.InitializeAsync(sp);

        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        var count = await db.Assets.CountAsync();

        Assert.Equal(10, count);
    }
}
