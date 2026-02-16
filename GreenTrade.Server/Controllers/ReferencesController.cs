using GreenTrade.Server.Data;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReferencesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReferencesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("warehouses")]
    public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses()
    {
        return await _context.Warehouses.Where(w => w.IsActive).ToListAsync();
    }

    [HttpGet("certifications")]
    public async Task<ActionResult<IEnumerable<Certification>>> GetCertifications()
    {
        return await _context.Certifications.ToListAsync();
    }

    [HttpGet("commodities")]
    public async Task<ActionResult<IEnumerable<Commodity>>> GetCommodities()
    {
        return await _context.Commodities.ToListAsync();
    }
}
