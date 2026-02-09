namespace Trading.Persistence;

public class Asset
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AssetType { get; set; } = "Stock"; // Stock, Crypto, ETF, Bond, etc.
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Portfolio
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = string.Empty; // maps to Identity user ID
    public string Name { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<PortfolioHolding> Holdings { get; set; } = [];
}

public class PortfolioHolding
{
    public long Id { get; set; }
    public int PortfolioId { get; set; }
    public int AssetId { get; set; }
    public decimal Quantity { get; set; }
    public decimal AverageCost { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Portfolio? Portfolio { get; set; }
    public Asset? Asset { get; set; }
}

public class Trade
{
    public long Id { get; set; }
    public int PortfolioId { get; set; }
    public int AssetId { get; set; }
    public string Side { get; set; } = "Buy"; // Buy or Sell
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    public Portfolio? Portfolio { get; set; }
    public Asset? Asset { get; set; }
}
