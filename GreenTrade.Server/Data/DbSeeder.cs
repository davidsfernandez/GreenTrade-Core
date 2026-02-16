using GreenTrade.Server.Data;
using GreenTrade.Shared.Enums;
using GreenTrade.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Seed Commodities
        if (!await context.Commodities.AnyAsync())
        {
            context.Commodities.AddRange(
                new Commodity { Name = "Café Arábica", TickerSymbol = "KC", UnitOfMeasure = "Saca (60kg)" },
                new Commodity { Name = "Café Conilon", TickerSymbol = "C8", UnitOfMeasure = "Saca (60kg)" },
                new Commodity { Name = "Dólar", TickerSymbol = "USDBRL", UnitOfMeasure = "BRL" }
            );
            await context.SaveChangesAsync();
        }

        // 2. Seed Admin User
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

        // 3. Seed Market Settings
        if (!await context.MarketSettings.AnyAsync())
        {
            context.MarketSettings.Add(new MarketSettings
            {
                CoffeeBasis = -10.00m,
                ServiceFeePercentage = 0.5m,
                UpdatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // 4. Seed Warehouses
        if (!await context.Warehouses.AnyAsync())
        {
            context.Warehouses.AddRange(
                new Warehouse { Name = "Cooxupé - Complexo Japy", City = "Guaxupé", State = "MG", IsActive = true, Address = "Rod. MG 450, km 35" },
                new Warehouse { Name = "Minasul Varginha", City = "Varginha", State = "MG", IsActive = true, Address = "Av. do Café, 1200" },
                new Warehouse { Name = "Expocacer", City = "Patrocínio", State = "MG", IsActive = true, Address = "Av. Faria Pereira, 3888" }
            );
            await context.SaveChangesAsync();
        }

        // 5. Seed Certifications
        if (!await context.Certifications.AnyAsync())
        {
            context.Certifications.AddRange(
                new Certification { Name = "Rainforest Alliance", Organization = "Rainforest Alliance", Description = "Sustainable agriculture standard." },
                new Certification { Name = "UTZ Certified", Organization = "UTZ", Description = "Sustainable farming of coffee." },
                new Certification { Name = "Fair Trade", Organization = "Fairtrade International", Description = "Fair prices and better working conditions." }
            );
            await context.SaveChangesAsync();
        }
    }
}
