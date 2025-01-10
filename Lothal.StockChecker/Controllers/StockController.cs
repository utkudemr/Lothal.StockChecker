using System.Text.Json;
using Lothal.StockChecker.Models;
using Lothal.StockChecker.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Lothal.StockChecker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController(IConnectionMultiplexer redis,ILogger<StockController> logger) : ControllerBase
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

        var stock = JsonSerializer.Deserialize<int>(stockJson);

        return Ok(stock);
    }

    [HttpPost("decrease")]
    public async Task<IActionResult> DecreaseStock([FromBody] StockCheckoutRequest request)
    {
        var stockKey = string.Format(CustomConstants.StockKey, request.Barcode);

        var stockJson = await _redisDb.StringGetAsync(stockKey);

        if (stockJson.IsNullOrEmpty)
        {
            logger.LogWarning("Product not found. {Id}",request.Id);
            return NotFound(new { Message = "Product not found." });
        }

        var stock = JsonSerializer.Deserialize<int>(stockJson);

        if (stock < request.Quantity)
        {
            logger.LogWarning("Insufficient stock. {Id}",request.Id);
            return BadRequest(new { Message = "Insufficient stock." });
        }

        stock -= request.Quantity;

        var updatedStockJson = JsonSerializer.Serialize(stock);
        var isSuccess = await _redisDb.StringSetAsync(stockKey, updatedStockJson);

        if (!isSuccess)
        {
            logger.LogWarning("Failed to update stock. {Id}",request.Id);
            return BadRequest(new { Message = "Failed to update stock." });
        }
        logger.LogInformation("Stock decreased successfully.. {Id}",request.Id);
        return Ok(new { Message = "Stock decreased successfully.", Stock = stock });
    }

    [HttpPost("decrease-lua")]
    public async Task<IActionResult> DecreaseStockLua([FromBody] StockCheckoutRequest request)
    {
        var stockKey = string.Format(CustomConstants.StockKey, request.Barcode);

        var script = @"
        local stockKey = KEYS[1]
        local decreaseAmount = tonumber(ARGV[1])

        local currentStock = tonumber(redis.call('GET', stockKey))

        if not currentStock then
           return 0
        end

        if currentStock < decreaseAmount then
           return 0
        end

        redis.call('SET', stockKey, currentStock - decreaseAmount)

        return currentStock - decreaseAmount
    ";
        try
        {
            var result = await _redisDb.ScriptEvaluateAsync(
                script,
                [stockKey],
                [request.Quantity]
            );
                
            if ((int)result <= 0)
            {
                logger.LogWarning("Failed to update stock. {Id}",request.Id);
                return BadRequest(new { Message = result.ToString() });
            }

            logger.LogInformation("Stock decreased successfully.. {Id}",request.Id);
            return Ok(new { Message = "Stock decreased successfully.", Stock = (int)result });
        }
        catch (Exception ex)
        {
            logger.LogError("An error occurred. {Id}",request.Id);
            return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
        }
    }
}