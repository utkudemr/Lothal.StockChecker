using System.Text.Json;
using Lothal.StockChecker.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Lothal.StockChecker;

[ApiController]
[Route("api/[controller]")]
public class StockController(IConnectionMultiplexer redis) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        var db = redis.GetDatabase();
        var productKey = product.Barcode; 

        var productJson = JsonSerializer.Serialize(product);

        var isSuccess = await db.StringSetAsync(productKey, productJson);
        
        if (isSuccess)
        {
            return Ok(new { Message = "Product added successfully." });
        }

        return BadRequest(new { Message = "Failed to add product." });
    }

    [HttpGet("{barcode}")]
    public async Task<IActionResult> GetProductByBarcode(string barcode)
    {
        var db = redis.GetDatabase();

        var productJson = await db.StringGetAsync(barcode);

        if (productJson.IsNullOrEmpty)
        {
            return NotFound(new { Message = "Product not found." });
        }
        
        var product = JsonSerializer.Deserialize<Product>(productJson);

        return Ok(product);
    }
}