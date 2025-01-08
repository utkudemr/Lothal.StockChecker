namespace Lothal.StockChecker.Models;

public class CreateProductRequest
{
    public string Barcode { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }

    public Product CreateProduct()
    {
        return new Product()
        {
            Barcode = Barcode,
            Name = Name,
            Id = Id
        };
    }
}