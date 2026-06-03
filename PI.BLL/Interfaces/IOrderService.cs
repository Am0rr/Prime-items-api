using PI.BLL.DTOs.Orders;

namespace PI.BLL.Interfaces;

public interface IOrderService
{
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, Guid userId, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<OrderResponse> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<IEnumerable<OrderResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
}