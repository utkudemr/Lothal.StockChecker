using Lothal.StockChecker.Models.BaseModels;

namespace Lothal.StockChecker.Models;

public class StockCheckoutRequest: BaseRequest
{
    public string Barcode { get; set; }
    public int Quantity { get; set; }
}