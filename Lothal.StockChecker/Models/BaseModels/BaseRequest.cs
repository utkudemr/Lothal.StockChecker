namespace Lothal.StockChecker.Models.BaseModels;

public abstract class BaseRequest
{
    public Guid? Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset? RequestDate { get; set; } = DateTimeOffset.Now;
}