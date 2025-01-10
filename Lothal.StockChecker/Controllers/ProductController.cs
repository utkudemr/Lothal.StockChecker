using System.Text.Json;
using Lothal.StockChecker.Models;
using Lothal.StockChecker.Models.Constants;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Lothal.StockChecker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IConnectionMultiplexer redis, ILogger<ProductController> logger) : ControllerBase
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
            var stockKey = string.Format(CustomConstants.StockKey, product.Barcode);
            var stockJson = JsonSerializer.Serialize(productRequest.Stock);
            await _redisDb.StringSetAsync(stockKey, stockJson);
            logger.LogInformation("Created product with Barcode: {product.Barcode}",product.Barcode);
            return Ok(new { Message = $"Product added successfully.{productRequest.Id}" });
        }
        logger.LogInformation("Failed to add product. {product.Barcode}",product.Barcode);
        return BadRequest(new { Message = $"Failed to add product.{product.Barcode}" });
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