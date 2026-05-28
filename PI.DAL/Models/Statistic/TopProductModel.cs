namespace PI.DAL.Models.Statistic;

public class TopProductModel
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalItemsSold { get; set; }
    public decimal TotalRevenue { get; set; }
}