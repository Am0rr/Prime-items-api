namespace PI.BLL.DTOs.Orders;

public record UpdateOrderStatusRequest
{
    public Guid Id { get; init; }
    public string Status { get; init; } = null!;
}