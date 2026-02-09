using Microsoft.EntityFrameworkCore;

namespace Trading.Persistence;

/// <summary>
/// Domain/business data context — separate from the Identity database.
/// Uses the "trading" schema to keep tables organized.
/// </summary>
public class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<PortfolioHolding> PortfolioHoldings => Set<PortfolioHolding>();
    public DbSet<Trade> Trades => Set<Trade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("trading");

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasIndex(e => e.Symbol).IsUnique();
            entity.Property(e => e.Symbol).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.AssetType).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("USD");
        });

        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.HasIndex(e => e.OwnerId);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OwnerId).HasMaxLength(450);
            entity.Property(e => e.Currency).HasMaxLength(10).HasDefaultValue("USD");
        });

        modelBuilder.Entity<PortfolioHolding>(entity =>
        {
            entity.HasIndex(e => new { e.PortfolioId, e.AssetId }).IsUnique();
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.AverageCost).HasPrecision(18, 4);

            entity.HasOne(e => e.Portfolio)
                .WithMany(p => p.Holdings)
                .HasForeignKey(e => e.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                .WithMany()
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasIndex(e => e.PortfolioId);
            entity.HasIndex(e => e.ExecutedAt);
            entity.Property(e => e.Side).HasMaxLength(10);
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
            entity.Property(e => e.Price).HasPrecision(18, 4);
            entity.Property(e => e.Total).HasPrecision(18, 4);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Portfolio)
                .WithMany()
                .HasForeignKey(e => e.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Asset)
                .WithMany()
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
