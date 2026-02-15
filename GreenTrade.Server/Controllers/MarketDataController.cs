using GreenTrade.Server.Services.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenTrade.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MarketDataController : ControllerBase
{
    private readonly IMarketDataProvider _provider;

    public MarketDataController(IMarketDataProvider provider)
    {
        _provider = provider;
    }

    [HttpGet("history/{ticker}")]
    public async Task<ActionResult<IEnumerable<ChartDataDto>>> GetHistory(string ticker, [FromQuery] string interval = "1h", [FromQuery] string range = "1d")
    {
        var history = await _provider.GetHistoryAsync(ticker, interval, range);
        return Ok(history);
    }
}
