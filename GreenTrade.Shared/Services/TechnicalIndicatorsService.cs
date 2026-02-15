namespace GreenTrade.Shared.Services;

public static class TechnicalIndicatorsService
{
    public static List<decimal> CalculateSMA(List<decimal> prices, int period)
    {
        var sma = new List<decimal>();
        for (int i = 0; i < prices.Count; i++)
        {
            if (i < period - 1)
            {
                sma.Add(0); // Not enough data
                continue;
            }

            decimal sum = 0;
            for (int j = 0; j < period; j++)
            {
                sum += prices[i - j];
            }
            sma.Add(sum / period);
        }
        return sma;
    }

    public static List<decimal> CalculateRSI(List<decimal> prices, int period = 14)
    {
        var rsi = new List<decimal>();
        if (prices.Count < period + 1)
        {
            return prices.Select(_ => 0m).ToList();
        }

        decimal avgGain = 0;
        decimal avgLoss = 0;

        // First RSI calculation
        for (int i = 1; i <= period; i++)
        {
            var change = prices[i] - prices[i - 1];
            if (change > 0) avgGain += change;
            else avgLoss -= change; // Make positive
        }

        avgGain /= period;
        avgLoss /= period;

        // Initial placeholder zeros
        for(int k=0; k<=period; k++) rsi.Add(0);

        decimal firstRS = avgLoss == 0 ? 100 : avgGain / avgLoss;
        decimal firstRSI = 100 - (100 / (1 + firstRS));
        rsi.Add(firstRSI); // RSI at index 'period' (wait, usually standard is to start having values later)
        // Correcting: RSI requires previous avgGain/Loss. 
        
        // Let's use a simpler iterative approach for list
        // This is a simplified implementation. Standard Wilders smoothing is preferred but Simple MA is often used for approximation.
        // We will restart with a cleaner loop for Wilders Smoothing.
        return CalculateRSIWilders(prices, period);
    }

    private static List<decimal> CalculateRSIWilders(List<decimal> prices, int period)
    {
        var rsiValues = new List<decimal>();
        
        if (prices.Count <= period)
            return prices.Select(_ => 0m).ToList();

        // Fill initial 0s
        for (int i = 0; i < period; i++) rsiValues.Add(0);

        decimal avgGain = 0;
        decimal avgLoss = 0;

        // First average
        for (int i = 1; i <= period; i++)
        {
            decimal change = prices[i] - prices[i - 1];
            if (change > 0) avgGain += change;
            else avgLoss -= change;
        }
        avgGain /= period;
        avgLoss /= period;

        decimal rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
        rsiValues.Add(100 - (100 / (1 + rs)));

        // Subsequent values
        for (int i = period + 1; i < prices.Count; i++)
        {
            decimal change = prices[i] - prices[i - 1];
            decimal gain = change > 0 ? change : 0;
            decimal loss = change < 0 ? -change : 0;

            avgGain = ((avgGain * (period - 1)) + gain) / period;
            avgLoss = ((avgLoss * (period - 1)) + loss) / period;

            rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            rsiValues.Add(100 - (100 / (1 + rs)));
        }

        return rsiValues;
    }
}
