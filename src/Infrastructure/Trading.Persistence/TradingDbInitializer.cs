using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Trading.Persistence;

/// <summary>
/// Ensures the TradingDb schema exists and seeds sample data in development.
/// </summary>
public static class TradingDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TradingDbContext>>();

        logger.LogInformation("Initializing TradingDb...");
        await db.Database.EnsureCreatedAsync();

        if (!await db.Assets.AnyAsync())
        {
            await SeedAssetsAsync(db, logger);
        }

        logger.LogInformation("TradingDb initialization complete");
    }

    private static async Task SeedAssetsAsync(TradingDbContext db, ILogger logger)
    {
        var assets = new[]
        {
            new Asset { Symbol = "AAPL", Name = "Apple Inc.", AssetType = "Stock", Currency = "USD" },
            new Asset { Symbol = "MSFT", Name = "Microsoft Corporation", AssetType = "Stock", Currency = "USD" },
            new Asset { Symbol = "GOOGL", Name = "Alphabet Inc.", AssetType = "Stock", Currency = "USD" },
            new Asset { Symbol = "AMZN", Name = "Amazon.com Inc.", AssetType = "Stock", Currency = "USD" },
            new Asset { Symbol = "TSLA", Name = "Tesla Inc.", AssetType = "Stock", Currency = "USD" },
            new Asset { Symbol = "BTC", Name = "Bitcoin", AssetType = "Crypto", Currency = "USD" },
            new Asset { Symbol = "ETH", Name = "Ethereum", AssetType = "Crypto", Currency = "USD" },
            new Asset { Symbol = "SPY", Name = "SPDR S&P 500 ETF", AssetType = "ETF", Currency = "USD" },
            new Asset { Symbol = "QQQ", Name = "Invesco QQQ Trust", AssetType = "ETF", Currency = "USD" },
            new Asset { Symbol = "NVDA", Name = "NVIDIA Corporation", AssetType = "Stock", Currency = "USD" },
        };

        db.Assets.AddRange(assets);
        await db.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} assets", assets.Length);
    }
}
