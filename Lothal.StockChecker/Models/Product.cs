namespace Lothal.StockChecker.Models;

public class Product
{
    public string Barcode { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }
}