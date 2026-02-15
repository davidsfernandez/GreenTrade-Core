using System.Security.Claims;
using GreenTrade.Server.Data;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PriceAlertsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PriceAlertsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PriceAlertDto>>> GetMyAlerts()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var alerts = await _context.PriceAlerts
            .Include(a => a.Commodity)
            .Where(a => a.UserId == userId)
            .Select(a => new PriceAlertDto
            {
                Id = a.Id,
                CommodityName = a.Commodity!.Name,
                Ticker = a.Commodity.TickerSymbol,
                TargetPrice = a.TargetPrice,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            })
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return Ok(alerts);
    }

    [HttpGet("commodities")]
    public async Task<ActionResult<IEnumerable<Commodity>>> GetCommodities()
    {
        return await _context.Commodities.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<PriceAlertDto>> CreateAlert(CreatePriceAlertDto request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (!await _context.Commodities.AnyAsync(c => c.Id == request.CommodityId))
        {
            return BadRequest("Commodity not found");
        }

        var alert = new PriceAlert
        {
            UserId = userId,
            CommodityId = request.CommodityId,
            TargetPrice = request.TargetPrice,
            IsActive = true
        };

        _context.PriceAlerts.Add(alert);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAlert(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var alert = await _context.PriceAlerts.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        if (alert == null) return NotFound();

        _context.PriceAlerts.Remove(alert);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
