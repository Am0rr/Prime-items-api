using AutoMapper;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;
using PI.BLL.DTOs.Orders;
using PI.DAL.Entities.Catalog;
using PI.DAL.Entities.Orders;
using PI.DAL.Enums;

namespace PI.BLL.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateOrderRequest request, Guid userId, CancellationToken cancellationToken)
    {
        if (request.Items is null)
            throw new ArgumentException("Order items must be provided.", nameof(request));

        var productsToUpdate = new List<Product>();

        var order = Order.Create(userId);

        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product with Id {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Not enough stock for product {product.Name}.");

            product.ChangeStockQuantity(product.StockQuantity - item.Quantity);
            productsToUpdate.Add(product);

            order.AddItem(product.Id, item.Quantity, product.Price);
        }

        foreach (var product in productsToUpdate)
        {
            _unitOfWork.Products.Update(product);
        }

        _unitOfWork.Orders.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    public async Task UpdateStatusAsync(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

        bool hasChanges = false;

        if (request.Status != null)
        {
            var status = Enum.Parse<OrderStatus>(request.Status, ignoreCase: true);

            if (status != order.Status)
            {
                order.ChangeStatus(status);
                hasChanges = true;
            }
        }

        if (!hasChanges) return;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        _unitOfWork.Orders.Delete(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<List<OrderResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);

        return _mapper.Map<List<OrderResponse>>(orders);
    }

    public async Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId, cancellationToken);

        return _mapper.Map<List<OrderResponse>>(orders);
    }
}