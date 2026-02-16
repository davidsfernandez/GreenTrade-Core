using GreenTrade.Shared.Services;
using Xunit;

namespace GreenTrade.Tests;

public class PriceCalculatorTests
{
    private readonly PriceCalculatorService _calculator = new();

    [Fact]
    public void CalculateCoffeeBagPrice_StandardValues_ReturnsCorrectPrice()
    {
        // Arrange
        decimal priceNy = 185.50m;
        decimal dollar = 5.25m;
        decimal basis = -10.00m;

        // Formula: (1.8550 * 132.2762 * 5.25) - 10 = 1278.204...
        decimal expected = 1278.20m;

        // Act
        decimal actual = _calculator.CalculateCoffeeBagPrice(priceNy, dollar, basis);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculateCoffeeBagPrice_WithAdjustment_ReturnsAdjustedPrice()
    {
        // Arrange
        decimal priceNy = 100.00m; // 1.00 USD/lb
        decimal dollar = 5.00m;
        decimal basis = 0m;
        decimal adjustment = 1.10m; // +10% premium

        // (1.00 * 132.2762 * 5.00) * 1.10 = 661.381 * 1.10 = 727.5191
        decimal expected = 727.52m;

        // Act
        decimal actual = _calculator.CalculateCoffeeBagPrice(priceNy, dollar, basis, adjustment);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 5, 0)]
    [InlineData(100, 0, 0)]
    [InlineData(-10, 5, 0)]
    public void CalculateCoffeeBagPrice_InvalidInputs_ReturnsZero(decimal priceNy, decimal dollar, decimal basis)
    {
        // Act
        decimal actual = _calculator.CalculateCoffeeBagPrice(priceNy, dollar, basis);

        // Assert
        Assert.Equal(0, actual);
    }
}
