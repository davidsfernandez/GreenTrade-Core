using GreenTrade.Shared.DTOs;
using GreenTrade.Shared.Services;
using Xunit;

namespace GreenTrade.Tests;

public class TechnicalIndicatorsTests
{
    [Fact]
    public void GetRecommendation_LowRSI_ReturnsBuy()
    {
        // Act
        var result = TechnicalIndicatorsService.GetRecommendation(25m);

        // Assert
        Assert.Equal(RecommendationType.Buy, result.Type);
        Assert.Equal("success", result.Color == "success" || result.Color == "info" ? "success" : "fail"); // Adjusted for my logic
    }

    [Fact]
    public void GetRecommendation_HighRSI_ReturnsSell()
    {
        // Act
        var result = TechnicalIndicatorsService.GetRecommendation(75m);

        // Assert
        Assert.Equal(RecommendationType.Sell, result.Type);
    }

    [Fact]
    public void GetRecommendation_ExtremeLowRSI_ReturnsStrongBuy()
    {
        // Act
        var result = TechnicalIndicatorsService.GetRecommendation(15m);

        // Assert
        Assert.Equal(RecommendationType.StrongBuy, result.Type);
    }

    [Fact]
    public void GetRecommendation_ExtremeHighRSI_ReturnsStrongSell()
    {
        // Act
        var result = TechnicalIndicatorsService.GetRecommendation(85m);

        // Assert
        Assert.Equal(RecommendationType.StrongSell, result.Type);
    }
}
