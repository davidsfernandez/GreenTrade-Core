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

        // Seed Warehouses
        if (!await context.Warehouses.AnyAsync())
        {
            context.Warehouses.AddRange(
                new Warehouse { Name = "Cooxupé - Complexo Japy", City = "Guaxupé", State = "MG", Address = "Rod. MG 450, km 35" },
                new Warehouse { Name = "Minasul Varginha", City = "Varginha", State = "MG", Address = "Av. do Café, 1200" },
                new Warehouse { Name = "Expocacer", City = "Patrocínio", State = "MG", Address = "Av. Faria Pereira, 3888" },
                new Warehouse { Name = "Armazém Gerais Leste de Minas", City = "Manhuaçu", State = "MG" }
            );
            await context.SaveChangesAsync();
        }

        // Seed Certifications
        if (!await context.Certifications.AnyAsync())
        {
            context.Certifications.AddRange(
                new Certification { Name = "Rainforest Alliance", Organization = "Rainforest Alliance", Description = "Sustainable agriculture standard." },
                new Certification { Name = "UTZ Certified", Organization = "UTZ", Description = "Sustainable farming of coffee." },
                new Certification { Name = "Fair Trade", Organization = "Fairtrade International", Description = "Fair prices and better working conditions." },
                new Certification { Name = "Orgânico Brasil", Organization = "MAPA", Description = "Produto orgânico certificado pelo governo brasileiro." },
                new Certification { Name = "Cerrado Mineiro", Organization = "Federação dos Cafeicultores do Cerrado", Description = "Denominação de Origem." },
                new Certification { Name = "4C", Organization = "4C Services", Description = "Common Code for the Coffee Community." }
            );
            await context.SaveChangesAsync();
        }
    }
}
