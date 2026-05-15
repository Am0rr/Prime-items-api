namespace PI.BLL.DTOs.Orders;

public record OrderItemResponse
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public string? ImageUrl { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}