using PI.BLL.DTOs.Orders;

namespace PI.BLL.Interfaces;

public interface IOrderService
{
    Task<Guid> CreateAsync(CreateOrderRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<List<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
}