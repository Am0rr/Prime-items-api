namespace PI.BLL.DTOs.Orders;

public record UpdateOrderStatusRequest(
    Guid Id,
    string? Status
);