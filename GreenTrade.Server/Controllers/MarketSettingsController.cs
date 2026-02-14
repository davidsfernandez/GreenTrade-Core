using GreenTrade.Server.Data;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Controllers;

/// <summary>
/// Controller for managing global market configuration.
/// Only Admins should be able to update these settings.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MarketSettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MarketSettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<MarketSettings>> GetSettings()
    {
        var settings = await _context.MarketSettings.FirstOrDefaultAsync();
        if (settings == null) return NotFound();
        return Ok(settings);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSettings(MarketSettings settings)
    {
        var existing = await _context.MarketSettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            settings.Id = 1;
            _context.MarketSettings.Add(settings);
        }
        else
        {
            existing.CoffeeBasis = settings.CoffeeBasis;
            existing.ServiceFeePercentage = settings.ServiceFeePercentage;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
