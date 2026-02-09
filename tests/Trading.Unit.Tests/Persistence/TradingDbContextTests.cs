using Microsoft.EntityFrameworkCore;
using Trading.Persistence;

namespace Trading.Unit.Tests.Persistence;

public class TradingDbContextTests : IDisposable
{
    private readonly TradingDbContext _context;

    public TradingDbContextTests()
    {
        var options = new DbContextOptionsBuilder<TradingDbContext>()
            .UseInMemoryDatabase($"TradingDbTest_{Guid.NewGuid()}")
            .Options;

        _context = new TradingDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void DbContext_InheritsFromDbContext()
    {
        Assert.IsAssignableFrom<DbContext>(_context);
    }

    [Fact]
    public void DbContext_HasAssetsDbSet()
    {
        Assert.NotNull(_context.Assets);
    }

    [Fact]
    public void DbContext_HasPortfoliosDbSet()
    {
        Assert.NotNull(_context.Portfolios);
    }

    [Fact]
    public void DbContext_HasPortfolioHoldingsDbSet()
    {
        Assert.NotNull(_context.PortfolioHoldings);
    }

    [Fact]
    public void DbContext_HasTradesDbSet()
    {
        Assert.NotNull(_context.Trades);
    }

    [Fact]
    public void CanAddAndRetrieveAsset()
    {
        var asset = new Asset
        {
            Symbol = "AAPL",
            Name = "Apple Inc.",
            AssetType = "Stock",
            Currency = "USD"
        };

        _context.Assets.Add(asset);
        _context.SaveChanges();

        var found = _context.Assets.FirstOrDefault(a => a.Symbol == "AAPL");
        Assert.NotNull(found);
        Assert.Equal("Apple Inc.", found.Name);
        Assert.Equal("Stock", found.AssetType);
        Assert.True(found.IsActive);
    }

    [Fact]
    public void CanAddAndRetrievePortfolio()
    {
        var portfolio = new Portfolio
        {
            OwnerId = "user-123",
            Name = "My Portfolio",
            Currency = "EUR"
        };

        _context.Portfolios.Add(portfolio);
        _context.SaveChanges();

        var found = _context.Portfolios.FirstOrDefault(p => p.OwnerId == "user-123");
        Assert.NotNull(found);
        Assert.Equal("My Portfolio", found.Name);
        Assert.Equal("EUR", found.Currency);
    }

    [Fact]
    public void CanAddHoldingToPortfolio()
    {
        var asset = new Asset { Symbol = "MSFT", Name = "Microsoft", AssetType = "Stock" };
        _context.Assets.Add(asset);

        var portfolio = new Portfolio { OwnerId = "user-456", Name = "Tech Portfolio" };
        _context.Portfolios.Add(portfolio);
        _context.SaveChanges();

        var holding = new PortfolioHolding
        {
            PortfolioId = portfolio.Id,
            AssetId = asset.Id,
            Quantity = 10.5m,
            AverageCost = 350.25m
        };

        _context.PortfolioHoldings.Add(holding);
        _context.SaveChanges();

        var found = _context.PortfolioHoldings
            .Include(h => h.Asset)
            .Include(h => h.Portfolio)
            .FirstOrDefault(h => h.PortfolioId == portfolio.Id);

        Assert.NotNull(found);
        Assert.Equal(10.5m, found.Quantity);
        Assert.Equal(350.25m, found.AverageCost);
        Assert.Equal("MSFT", found.Asset!.Symbol);
        Assert.Equal("Tech Portfolio", found.Portfolio!.Name);
    }

    [Fact]
    public void CanAddAndRetrieveTrade()
    {
        var asset = new Asset { Symbol = "TSLA", Name = "Tesla", AssetType = "Stock" };
        _context.Assets.Add(asset);

        var portfolio = new Portfolio { OwnerId = "user-789", Name = "Growth" };
        _context.Portfolios.Add(portfolio);
        _context.SaveChanges();

        var trade = new Trade
        {
            PortfolioId = portfolio.Id,
            AssetId = asset.Id,
            Side = "Buy",
            Quantity = 5m,
            Price = 200.50m,
            Total = 1002.50m,
            Notes = "Initial position"
        };

        _context.Trades.Add(trade);
        _context.SaveChanges();

        var found = _context.Trades
            .Include(t => t.Asset)
            .FirstOrDefault(t => t.PortfolioId == portfolio.Id);

        Assert.NotNull(found);
        Assert.Equal("Buy", found.Side);
        Assert.Equal(5m, found.Quantity);
        Assert.Equal(200.50m, found.Price);
        Assert.Equal(1002.50m, found.Total);
        Assert.Equal("TSLA", found.Asset!.Symbol);
    }

    [Fact]
    public void Portfolio_Holdings_NavigationWorks()
    {
        var asset1 = new Asset { Symbol = "SPY", Name = "S&P 500 ETF", AssetType = "ETF" };
        var asset2 = new Asset { Symbol = "QQQ", Name = "Nasdaq ETF", AssetType = "ETF" };
        _context.Assets.AddRange(asset1, asset2);

        var portfolio = new Portfolio { OwnerId = "user-nav", Name = "ETF Portfolio" };
        _context.Portfolios.Add(portfolio);
        _context.SaveChanges();

        _context.PortfolioHoldings.AddRange(
            new PortfolioHolding { PortfolioId = portfolio.Id, AssetId = asset1.Id, Quantity = 100, AverageCost = 450 },
            new PortfolioHolding { PortfolioId = portfolio.Id, AssetId = asset2.Id, Quantity = 50, AverageCost = 380 }
        );
        _context.SaveChanges();

        var loaded = _context.Portfolios
            .Include(p => p.Holdings)
            .ThenInclude(h => h.Asset)
            .First(p => p.Id == portfolio.Id);

        Assert.Equal(2, loaded.Holdings.Count);
        Assert.Contains(loaded.Holdings, h => h.Asset!.Symbol == "SPY");
        Assert.Contains(loaded.Holdings, h => h.Asset!.Symbol == "QQQ");
    }

    [Fact]
    public void Asset_DefaultValues_AreCorrect()
    {
        var asset = new Asset { Symbol = "TEST", Name = "Test Asset" };

        Assert.Equal("Stock", asset.AssetType);
        Assert.Equal("USD", asset.Currency);
        Assert.True(asset.IsActive);
    }

    [Fact]
    public void Trade_DefaultSide_IsBuy()
    {
        var trade = new Trade();
        Assert.Equal("Buy", trade.Side);
    }

    [Fact]
    public void Portfolio_DefaultCurrency_IsUSD()
    {
        var portfolio = new Portfolio();
        Assert.Equal("USD", portfolio.Currency);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
