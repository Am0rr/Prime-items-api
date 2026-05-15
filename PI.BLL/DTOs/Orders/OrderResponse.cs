namespace PI.BLL.DTOs.Orders;

public record OrderResponse
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Status { get; init; } = null!;
    public decimal TotalAmount { get; init; }
    public List<OrderItemResponse> Items { get; init; } = new();
}