namespace GreenTrade.Shared.Enums;

public enum OfferStatus
{
    Pending = 0,    // Offer made, waiting for seller response
    Accepted = 1,   // Seller accepted the offer
    Rejected = 2,   // Seller rejected the offer
    Countered = 3,  // Seller made a counter-offer (future feature)
    Cancelled = 4   // Buyer cancelled the offer
}
