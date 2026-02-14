using GreenTrade.Server.Data;
using GreenTrade.Shared.Enums;
using GreenTrade.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Seed Commodities
        if (!await context.Commodities.AnyAsync())
        {
            context.Commodities.AddRange(
                new Commodity { Name = "Arabica Coffee", TickerSymbol = "KC", UnitOfMeasure = "Bag (60kg)" },
                new Commodity { Name = "Conilon Coffee", TickerSymbol = "C8", UnitOfMeasure = "Bag (60kg)" },
                new Commodity { Name = "US Dollar", TickerSymbol = "USDBRL", UnitOfMeasure = "BRL" }
            );
            await context.SaveChangesAsync();
        }

        // Seed Admin User
        if (!await context.Users.AnyAsync())
        {
            var admin = new User
            {
                FullName = "System Admin",
                Email = "admin@greentrade.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }

        // Seed Market Settings
        if (!await context.MarketSettings.AnyAsync())
        {
            context.MarketSettings.Add(new MarketSettings
            {
                CoffeeBasis = -10.00m, // Example: -10 BRL discount relative to exchange
                ServiceFeePercentage = 0.5m
            });
            await context.SaveChangesAsync();
        }
    }
}
