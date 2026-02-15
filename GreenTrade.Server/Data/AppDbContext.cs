using Microsoft.EntityFrameworkCore;
using GreenTrade.Shared.Models;
using GreenTrade.Shared.Enums;

namespace GreenTrade.Server.Data;

/// <summary>
/// Database context for GreenTrade Core platform.
/// Handles Users, Commodities, Price Alerts, and Coffee Lots.
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
    public DbSet<CoffeeLot> CoffeeLots { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Certification> Certifications { get; set; }
    public DbSet<LotCertification> LotCertifications { get; set; }
    public DbSet<Offer> Offers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure pluralization or manual table names if needed
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Commodity>().ToTable("Commodities");
        modelBuilder.Entity<PriceAlert>().ToTable("PriceAlerts");
        modelBuilder.Entity<MarketSettings>().ToTable("MarketSettings");
        modelBuilder.Entity<CoffeeLot>().ToTable("CoffeeLots");
        modelBuilder.Entity<Warehouse>().ToTable("Warehouses");
        modelBuilder.Entity<Certification>().ToTable("Certifications");
        modelBuilder.Entity<LotCertification>().ToTable("LotCertifications");
        modelBuilder.Entity<Offer>().ToTable("Offers");

        // Relationships
        modelBuilder.Entity<PriceAlert>()
            .HasOne(p => p.User)
            .WithMany(u => u.PriceAlerts)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<PriceAlert>()
            .HasOne(p => p.Commodity)
            .WithMany(c => c.PriceAlerts)
            .HasForeignKey(p => p.CommodityId);

        // Offer Relationships
        modelBuilder.Entity<Offer>()
            .HasOne(o => o.CoffeeLot)
            .WithMany()
            .HasForeignKey(o => o.CoffeeLotId)
            .OnDelete(DeleteBehavior.Cascade); // If lot is deleted, offers are deleted

        modelBuilder.Entity<Offer>()
            .HasOne(o => o.Buyer)
            .WithMany()
            .HasForeignKey(o => o.BuyerId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete user if offer is deleted, vice versa handled by DB usually but restrict is safer to prevent accidental user deletion if offers exist, though here usually Cascade is fine for User->Offer. Let's stick to Restrict or NoAction to avoid cycles if any. Actually, User owns offers? No, User is the Buyer.

        // Many-to-Many: CoffeeLot <-> Certification
        modelBuilder.Entity<LotCertification>()
            .HasKey(lc => new { lc.CoffeeLotId, lc.CertificationId });

        modelBuilder.Entity<LotCertification>()
            .HasOne(lc => lc.CoffeeLot)
            .WithMany(l => l.LotCertifications)
            .HasForeignKey(lc => lc.CoffeeLotId);

        modelBuilder.Entity<LotCertification>()
            .HasOne(lc => lc.Certification)
            .WithMany()
            .HasForeignKey(lc => lc.CertificationId);
    }
}
