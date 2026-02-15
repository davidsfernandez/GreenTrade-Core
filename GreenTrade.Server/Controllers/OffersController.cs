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
public class OffersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OffersController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/offers (Buyer makes an offer)
    [HttpPost]
    public async Task<ActionResult<Offer>> CreateOffer(CreateOfferDto request)
    {
        var buyerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var lot = await _context.CoffeeLots.FindAsync(request.CoffeeLotId);
        if (lot == null) return NotFound("Lote não encontrado.");

        if (lot.UserId == buyerId) return BadRequest("Você não pode ofertar em seu próprio lote.");

        var offer = new Offer
        {
            CoffeeLotId = request.CoffeeLotId,
            BuyerId = buyerId,
            PricePerBag = request.PricePerBag,
            Quantity = lot.Quantity, // Full lot offer for MVP
            Status = OfferStatus.Pending,
            Remarks = request.Remarks,
            CreatedAt = DateTime.UtcNow
        };

        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();

        // TODO: Notify Seller (SignalR/Email)

        return Ok(new { Message = "Oferta enviada com sucesso!" });
    }

    // GET: api/offers/my-sent (Buyer sees sent offers)
    [HttpGet("my-sent")]
    public async Task<ActionResult<IEnumerable<OfferDto>>> GetMySentOffers()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        return await _context.Offers
            .Include(o => o.CoffeeLot)
            .ThenInclude(l => l.Commodity)
            .Where(o => o.BuyerId == userId)
            .Select(o => new OfferDto
            {
                Id = o.Id,
                CoffeeLotId = o.CoffeeLotId,
                CoffeeLotDescription = $"{o.CoffeeLot!.Quantity} sacas de {o.CoffeeLot.Commodity!.Name}",
                BuyerId = o.BuyerId,
                BuyerName = "Você",
                PricePerBag = o.PricePerBag,
                Quantity = o.Quantity,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    // GET: api/offers/received (Seller sees received offers)
    [HttpGet("received")]
    public async Task<ActionResult<IEnumerable<OfferDto>>> GetReceivedOffers()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Find lots owned by user
        var myLotIds = await _context.CoffeeLots
            .Where(l => l.UserId == userId)
            .Select(l => l.Id)
            .ToListAsync();

        return await _context.Offers
            .Include(o => o.CoffeeLot)
            .ThenInclude(l => l.Commodity)
            .Include(o => o.Buyer)
            .Where(o => myLotIds.Contains(o.CoffeeLotId))
            .Select(o => new OfferDto
            {
                Id = o.Id,
                CoffeeLotId = o.CoffeeLotId,
                CoffeeLotDescription = $"{o.CoffeeLot!.Quantity} sacas de {o.CoffeeLot.Commodity!.Name}",
                BuyerId = o.BuyerId,
                BuyerName = o.Buyer!.FullName,
                PricePerBag = o.PricePerBag,
                Quantity = o.Quantity,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    // POST: api/offers/{id}/respond (Seller Accepts/Rejects)
    [HttpPost("{id}/respond")]
    public async Task<IActionResult> RespondToOffer(int id, UpdateOfferStatusDto response)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var offer = await _context.Offers
            .Include(o => o.CoffeeLot)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null) return NotFound();

        // Verify ownership
        if (offer.CoffeeLot!.UserId != userId) return Forbid();

        if (offer.Status != OfferStatus.Pending)
            return BadRequest("Oferta já processada.");

        if (response.IsAccepted)
        {
            offer.Status = OfferStatus.Accepted;
            offer.RespondedAt = DateTime.UtcNow;
            
            // Update Lot Status
            offer.CoffeeLot.Status = LotStatus.UnderOffer; // Or Sold? Let's say UnderOffer until payment.
        }
        else
        {
            offer.Status = OfferStatus.Rejected;
            offer.RespondedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }
}
