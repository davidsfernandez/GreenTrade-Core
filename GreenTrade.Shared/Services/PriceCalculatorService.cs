namespace GreenTrade.Shared.Services;

/// <summary>
/// Service for financial calculations related to Agricultural Commodities.
/// Follows industry standard formulas for conversion.
/// </summary>
public interface IPriceCalculatorService
{
    /// <summary>
    /// Calculates the suggested price for a 60kg bag in BRL.
    /// Formula: (PriceNY * 1.32276 * DollarRate) + Basis
    /// Explanation: 
    /// 1 bag = 60 kg = 132.276 lbs.
    /// Since PriceNY is in Centavos/lb, we divide by 100 to get Dollars/lb.
    /// (PriceNY / 100) * 132.276 * DollarRate = Price per bag in BRL.
    /// Simplified: PriceNY * 1.32276 * DollarRate.
    /// </summary>
    decimal CalculateCoffeeBagPrice(decimal priceNyCentavos, decimal dollarRate, decimal basisBrl, decimal adjustmentFactor = 1.0m);
}

public class PriceCalculatorService : IPriceCalculatorService
{
    private const decimal LbsIn60KgBag = 132.2762m;

    public decimal CalculateCoffeeBagPrice(decimal priceNyCentavos, decimal dollarRate, decimal basisBrl, decimal adjustmentFactor = 1.0m)
    {
        if (priceNyCentavos <= 0 || dollarRate <= 0) return 0;

        // Convert Centavos/lb to Dollars/lb
        decimal priceUsdPerLb = priceNyCentavos / 100m;

        // Convert Dollars/lb to Dollars/60kg Bag
        decimal priceUsdPerBag = priceUsdPerLb * LbsIn60KgBag;

        // Convert Dollars/Bag to BRL/Bag
        decimal priceBrlPerBag = priceUsdPerBag * dollarRate;

        // Apply Basis (Differential) and Adjustment
        decimal finalPrice = (priceBrlPerBag + basisBrl) * adjustmentFactor;

        return Math.Round(finalPrice, 2);
    }
}
