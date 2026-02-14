using Microsoft.EntityFrameworkCore;
using GreenTrade.Shared.Models;

namespace GreenTrade.Server.Data;

/// <summary>
/// Database context for GreenTrade Core platform.
/// Handles Users, Commodities, and Price Alerts.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Commodity> Commodities { get; set; }
    public DbSet<PriceAlert> PriceAlerts { get; set; }
    public DbSet<MarketSettings> MarketSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure pluralization or manual table names if needed
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Commodity>().ToTable("Commodities");
        modelBuilder.Entity<PriceAlert>().ToTable("PriceAlerts");
        modelBuilder.Entity<MarketSettings>().ToTable("MarketSettings");

        // Relationships are already defined by DataAnnotations/Conventions, 
        // but we can be explicit here for better clarity.
        modelBuilder.Entity<PriceAlert>()
            .HasOne(p => p.User)
            .WithMany(u => u.PriceAlerts)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<PriceAlert>()
            .HasOne(p => p.Commodity)
            .WithMany(c => c.PriceAlerts)
            .HasForeignKey(p => p.CommodityId);
    }
}
