using AutoMapper;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.BLL.DTOs.Orders;
using PI.BLL.Exceptions;
using PI.DAL.Entities.Orders;
using PI.DAL.Enums;

namespace PI.BLL.Services;

public class OrderService : BaseService, IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, Guid userId, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);

        var order = new Order(userId);

        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with Id {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Not enough stock for product {product.Name}.");

            product.ChangeStockQuantity(product.StockQuantity - item.Quantity);

            order.AddItem(product.Id, item.Quantity, product.Price);
        }

        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        await ValidateAsync(request);

        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

        if (request.Status != null)
        {
            var status = Enum.Parse<OrderStatus>(request.Status, ignoreCase: true);

            if (status != order.Status)
            {
                order.ChangeStatus(status);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        _unitOfWork.Orders.Delete(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderResponse> GetByIdAsync(Guid orderId, Guid currentUserId, string role, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        if (!HasGlobalAccess(role) && order.UserId != currentUserId)
            throw new ForbiddenException("You are not allowed to access this order.");

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);

        return _mapper.Map<IEnumerable<OrderResponse>>(orders);
    }

    public async Task<IEnumerable<OrderResponse>> GetUserOrdersAsync(Guid userId, Guid currentUserId, string role, CancellationToken cancellationToken)
    {
        if (!HasGlobalAccess(role) && userId != currentUserId)
            throw new ForbiddenException("You are not allowed to access these orders.");

        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId, cancellationToken);

        return _mapper.Map<IEnumerable<OrderResponse>>(orders);
    }

    private static bool HasGlobalAccess(string role) =>
        string.Equals(role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, nameof(UserRole.Manager), StringComparison.OrdinalIgnoreCase);
}