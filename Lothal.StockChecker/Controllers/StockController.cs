using System.Text.Json;
using Lothal.StockChecker.Models;
using Lothal.StockChecker.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Lothal.StockChecker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController(IConnectionMultiplexer redis) : ControllerBase
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    [HttpGet("stock/{barcode}")]
    public async Task<IActionResult> GetStockByBarcode(string barcode)
    {
        var stockKey = string.Format(CustomConstants.StockKey, barcode);
        var stockJson = await _redisDb.StringGetAsync(stockKey);

        if (stockJson.IsNullOrEmpty)
        {
            return NotFound(new { Message = "Product not found." });
        }

        var stock = JsonSerializer.Deserialize<Stock>(stockJson);

        return Ok(stock);
    }
    
    [HttpPost("decrease")]
    public async Task<IActionResult> DecreaseStock([FromBody] StockCheckoutRequest request)
    {
        var stockKey = string.Format(CustomConstants.StockKey, request.Barcode);

        var stockJson = await _redisDb.StringGetAsync(stockKey);

        if (stockJson.IsNullOrEmpty)
        {
            return NotFound(new { Message = "Product not found." });
        }

        var stock = JsonSerializer.Deserialize<Stock>(stockJson);

        if (stock.Quantity < request.Quantity)
        {
            return BadRequest(new { Message = "Insufficient stock." });
        }

        stock.Quantity -= request.Quantity;

        var updatedStockJson = JsonSerializer.Serialize(stock);
        var isSuccess = await _redisDb.StringSetAsync(stockKey, updatedStockJson);

        if (!isSuccess)
        {
            return BadRequest(new { Message = "Failed to update stock." });
        }

        return Ok(new { Message = "Stock decreased successfully.", Stock = stock.Quantity });
    }
}