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
        // Price NY: 185.50 cts/lb
        // USD/BRL: 5.25
        // Basis: -10.00 BRL
        decimal priceNy = 185.50m;
        decimal dollar = 5.25m;
        decimal basis = -10.00m;

        // Formula: (185.50 / 100) * 132.2762 * 5.25 - 10
        // (1.8550 * 132.2762 * 5.25) - 10
        // (245.3723... * 5.25) - 10
        // 1288.204... - 10 = 1278.204...
        decimal expected = 1278.20m;

        // Act
        decimal actual = _calculator.CalculateCoffeeBagPrice(priceNy, dollar, basis);

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
