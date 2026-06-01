namespace PI.BLL.DTOs.Orders;

public record OrderItemRequest(
    Guid ProductId,
    int Quantity
);