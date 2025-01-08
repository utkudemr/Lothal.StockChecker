namespace Lothal.StockChecker.Models;

public class StockCheckoutRequest
{
    public string Barcode { get; set; }
    public int Quantity { get; set; }
}