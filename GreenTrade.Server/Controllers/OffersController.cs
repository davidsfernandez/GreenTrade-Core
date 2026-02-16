using System.Security.Claims;
using GreenTrade.Server.Data;
using GreenTrade.Server.Hubs;
using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Enums;
using GreenTrade.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GreenTrade.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OffersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IHubContext<MarketHub> _hubContext;

    public OffersController(AppDbContext context, IHubContext<MarketHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // POST: api/offers (Buyer makes an offer)
    [HttpPost]
    public async Task<ActionResult<Offer>> CreateOffer(CreateOfferDto request)
    {
        var buyerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var lot = await _context.CoffeeLots
            .Include(l => l.Commodity)
            .FirstOrDefaultAsync(l => l.Id == request.CoffeeLotId);
            
        if (lot == null) return NotFound("Lote não encontrado.");

        if (lot.UserId == buyerId) return BadRequest("Você não puede ofertar em seu próprio lote.");

        var offer = new Offer
        {
            CoffeeLotId = request.CoffeeLotId,
            BuyerId = buyerId,
            PricePerBag = request.PricePerBag,
            Quantity = lot.Quantity, // Full lot offer for MVP
            Status = OfferStatus.Pending,
            Remarks = request.Remarks,
            CreatedAt = DateTime.UtcNow,
            LastModifiedById = buyerId // Buyer starts the negotiation
        };

        _context.Offers.Add(offer);
        await _context.SaveChangesAsync();

        // Notify Seller
        var message = $"Nova proposta recebida para seu lote de {lot.Commodity?.Name}!";
        await _hubContext.Clients.User(lot.UserId.ToString()).SendAsync("ReceiveAlert", message);

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
            .Include(o => o.CoffeeLot)
                .ThenInclude(l => l.User)
            .Include(o => o.CoffeeLot)
                .ThenInclude(l => l.Warehouse)
            .Where(o => o.BuyerId == userId)
            .Select(o => new OfferDto
            {
                Id = o.Id,
                CoffeeLotId = o.CoffeeLotId,
                CoffeeLotDescription = $"{o.CoffeeLot!.Quantity} sacas de {o.CoffeeLot.Commodity!.Name}",
                BuyerId = o.BuyerId,
                BuyerName = "Você",
                SellerName = o.CoffeeLot.User!.FullName,
                CommodityName = o.CoffeeLot.Commodity.Name,
                WarehouseName = o.CoffeeLot.Warehouse != null ? o.CoffeeLot.Warehouse.Name : "N/A",
                WarehouseCity = o.CoffeeLot.Warehouse != null ? o.CoffeeLot.Warehouse.City : "N/A",
                PricePerBag = o.PricePerBag,
                Quantity = o.Quantity,
                Status = o.Status.ToString(),
                Remarks = o.Remarks,
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

        return await _context.Offers
            .Include(o => o.CoffeeLot)
                .ThenInclude(l => l.Commodity)
            .Include(o => o.CoffeeLot)
                .ThenInclude(l => l.Warehouse)
            .Include(o => o.Buyer)
            .Where(o => o.CoffeeLot!.UserId == userId)
            .Select(o => new OfferDto
            {
                Id = o.Id,
                CoffeeLotId = o.CoffeeLotId,
                CoffeeLotDescription = $"{o.CoffeeLot!.Quantity} sacas de {o.CoffeeLot.Commodity!.Name}",
                BuyerId = o.BuyerId,
                BuyerName = o.Buyer!.FullName,
                SellerName = "Você",
                CommodityName = o.CoffeeLot.Commodity.Name,
                WarehouseName = o.CoffeeLot.Warehouse != null ? o.CoffeeLot.Warehouse.Name : "N/A",
                WarehouseCity = o.CoffeeLot.Warehouse != null ? o.CoffeeLot.Warehouse.City : "N/A",
                PricePerBag = o.PricePerBag,
                Quantity = o.Quantity,
                Status = o.Status.ToString(),
                Remarks = o.Remarks,
                CreatedAt = o.CreatedAt
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    // POST: api/offers/{id}/respond (Seller or Buyer responds)
    [HttpPost("{id}/respond")]
    public async Task<IActionResult> RespondToOffer(int id, UpdateOfferStatusDto response)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var offer = await _context.Offers
            .Include(o => o.CoffeeLot)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null) return NotFound();

        // Security: Only Seller or Buyer can respond
        bool isSeller = offer.CoffeeLot!.UserId == userId;
        bool isBuyer = offer.BuyerId == userId;

        if (!isSeller && !isBuyer) return Forbid();

        // Business Logic: You can't respond to your own last move
        if (offer.LastModifiedById == userId)
            return BadRequest("Aguardando resposta da outra parte.");

        if (offer.Status == OfferStatus.Accepted || offer.Status == OfferStatus.Rejected)
            return BadRequest("Esta negociação já foi encerrada.");

        // Parse New Status
        if (!Enum.TryParse<OfferStatus>(response.NewStatus, true, out var newStatus))
            return BadRequest("Status inválido.");

        offer.Status = newStatus;
        offer.RespondedAt = DateTime.UtcNow;
        offer.LastModifiedById = userId;
        offer.Remarks = response.Remarks;

        if (newStatus == OfferStatus.Accepted)
        {
            offer.CoffeeLot.Status = LotStatus.UnderOffer; // Mark lot as engaged
        }
        else if (newStatus == OfferStatus.Countered)
        {
            if (response.CounterPrice == null || response.CounterPrice <= 0)
                return BadRequest("O preço de contraproposta é obrigatório.");
            
            offer.PricePerBag = response.CounterPrice.Value;
        }

        await _context.SaveChangesAsync();

        // Notify the other party
        int targetUserId = isSeller ? offer.BuyerId : offer.CoffeeLot.UserId;
        string statusText = newStatus switch {
            OfferStatus.Accepted => "ACEITA",
            OfferStatus.Rejected => "RECUSADA",
            OfferStatus.Countered => "CONTRA-OFERTADA",
            _ => newStatus.ToString()
        };
        
        var notifyMsg = $"Sua proposta para o lote de {offer.CoffeeLot.Commodity?.Name} foi {statusText}.";
        await _hubContext.Clients.User(targetUserId.ToString()).SendAsync("ReceiveAlert", notifyMsg);

        return Ok(new { Message = "Resposta enviada com sucesso!", Status = offer.Status.ToString() });
    }

    // DELETE: api/offers/{id} (Buyer cancels their offer)
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOffer(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var offer = await _context.Offers
            .Include(o => o.CoffeeLot)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null) return NotFound();

        // Security: Only the Buyer can cancel their own offer
        if (offer.BuyerId != userId) return Forbid();

        if (offer.Status == OfferStatus.Accepted || offer.Status == OfferStatus.Rejected)
            return BadRequest("Não é possível cancelar uma negociação já encerrada.");

        if (offer.Status == OfferStatus.Cancelled)
            return BadRequest("Esta oferta já foi cancelada.");

        offer.Status = OfferStatus.Cancelled;
        offer.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Notify Seller
        var notifyMsg = $"A proposta para seu lote de {offer.CoffeeLot?.Commodity?.Name} foi CANCELADA pelo comprador.";
        await _hubContext.Clients.User(offer.CoffeeLot!.UserId.ToString()).SendAsync("ReceiveAlert", notifyMsg);

        return NoContent();
    }
}
