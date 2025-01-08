using System.Text.Json;
using Lothal.StockChecker.Models;
using Lothal.StockChecker.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Lothal.StockChecker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IConnectionMultiplexer redis) : ControllerBase
{
    private readonly IDatabase _redisDb = redis.GetDatabase();

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest productRequest)
    {
        var product = productRequest.CreateProduct();
        var productKey = product.Barcode;

        var productJson = JsonSerializer.Serialize(product);

        var isSuccess = await _redisDb.StringSetAsync(productKey, productJson);

        if (isSuccess)
        {
            var stock = new Stock()
            {
                ProductId = product.Id,
                Quantity = productRequest.Stock
            };
            var stockKey = string.Format(CustomConstants.StockKey, product.Barcode);
            var stockJson = JsonSerializer.Serialize(stock);
            await _redisDb.StringSetAsync(stockKey, stockJson);
            return Ok(new { Message = "Product added successfully." });
        }

        return BadRequest(new { Message = "Failed to add product." });
    }

    [HttpGet("{barcode}")]
    public async Task<IActionResult> GetProductByBarcode(string barcode)
    {
        var productJson = await _redisDb.StringGetAsync(barcode);

        if (productJson.IsNullOrEmpty)
        {
            return NotFound(new { Message = "Product not found." });
        }

        var product = JsonSerializer.Deserialize<Product>(productJson);

        return Ok(product);
    }

}