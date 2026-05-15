namespace PI.BLL.DTOs.Orders;

public record CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; init; } = new();
}