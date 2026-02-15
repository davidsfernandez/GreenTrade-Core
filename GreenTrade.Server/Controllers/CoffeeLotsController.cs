using System.Security.Claims;
using GreenTrade.Server.Data;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Enums;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CoffeeLotsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CoffeeLotsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/coffeelots (Marketplace - All active lots)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoffeeLotDto>>> GetMarketplaceLots()
    {
        return await _context.CoffeeLots
            .Include(l => l.Commodity)
            .Include(l => l.Warehouse)
            .Where(l => l.Status == LotStatus.Published)
            .Select(l => new CoffeeLotDto
            {
                Id = l.Id,
                CommodityName = l.Commodity!.Name,
                Quantity = l.Quantity,
                CropYear = l.CropYear,
                Quality = l.Quality.ToString(),
                WarehouseName = l.Warehouse != null ? l.Warehouse.Name : "N/A",
                WarehouseCity = l.Warehouse != null ? l.Warehouse.City : "N/A",
                AskingPrice = l.AskingPrice,
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt
            })
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    // GET: api/coffeelots/my (My Lots)
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<CoffeeLotDto>>> GetMyLots()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        return await _context.CoffeeLots
            .Include(l => l.Commodity)
            .Include(l => l.Warehouse)
            .Where(l => l.UserId == userId)
            .Select(l => new CoffeeLotDto
            {
                Id = l.Id,
                CommodityName = l.Commodity!.Name,
                Quantity = l.Quantity,
                CropYear = l.CropYear,
                Quality = l.Quality.ToString(),
                WarehouseName = l.Warehouse != null ? l.Warehouse.Name : "N/A",
                WarehouseCity = l.Warehouse != null ? l.Warehouse.City : "N/A",
                AskingPrice = l.AskingPrice,
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt
            })
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    // POST: api/coffeelots
    [HttpPost]
    public async Task<ActionResult<CoffeeLot>> CreateLot(CreateCoffeeLotDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Validate commodity
        if (!await _context.Commodities.AnyAsync(c => c.Id == request.CommodityId))
        {
            return BadRequest("Commodity invalida.");
        }

        // Validate Warehouse
        if (!await _context.Warehouses.AnyAsync(w => w.Id == request.WarehouseId))
        {
            return BadRequest("Armazém inválido.");
        }

        var lot = new CoffeeLot
        {
            UserId = userId,
            CommodityId = request.CommodityId,
            Quantity = request.Quantity,
            CropYear = request.CropYear,
            Quality = request.Quality,
            ScreenSize = request.ScreenSize,
            Defects = request.Defects,
            WarehouseId = request.WarehouseId,
            AskingPrice = request.AskingPrice,
            Description = request.Description,
            Status = LotStatus.Published, // Auto-publish for MVP
        };

        // Add Certifications
        if (request.CertificationIds != null && request.CertificationIds.Any())
        {
            foreach (var certId in request.CertificationIds)
            {
                // Verify if cert exists to avoid FK error, or let DB handle it? Better check.
                if (await _context.Certifications.AnyAsync(c => c.Id == certId))
                {
                    lot.LotCertifications.Add(new LotCertification
                    {
                        CertificationId = certId
                    });
                }
            }
        }

        _context.CoffeeLots.Add(lot);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyLots), new { id = lot.Id }, lot);
    }
    
    // DELETE: api/coffeelots/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLot(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var lot = await _context.CoffeeLots.FirstOrDefaultAsync(l => l.Id == id && l.UserId == userId);

        if (lot == null) return NotFound();

        _context.CoffeeLots.Remove(lot);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
