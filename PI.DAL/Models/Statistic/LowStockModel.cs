namespace PI.DAL.Models.Statistic;

public class LowStockModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}