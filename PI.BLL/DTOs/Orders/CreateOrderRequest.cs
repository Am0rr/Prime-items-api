namespace PI.BLL.DTOs.Orders;

public record CreateOrderRequest(
    List<OrderItemRequest> Items
);