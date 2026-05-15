namespace PI.BLL.DTOs.Orders;

public record OrderItemRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}