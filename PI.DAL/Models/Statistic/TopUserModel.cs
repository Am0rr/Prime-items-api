namespace PI.DAL.Models.Statistic;

public class TopUserModel
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int TotalOrders { get; set; }
}