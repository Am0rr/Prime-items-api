using AutoMapper;
using PI.BLL.DTOs.Orders;
using PI.BLL.Interfaces;
using PI.DAL.Entities.Orders;
using PI.DAL.Interfaces;
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
        decimal totalAmount = 0;
        var itemsData = new List<(Guid ProductId, int Quantity, decimal UnitPrice)>();
        var productsToUpdate = new List<PI.DAL.Entities.Catalog.Product>();

        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIDAsync(item.ProductId)
                ?? throw new KeyNotFoundException($"Product with Id {item.ProductId} not found.");

            if (product.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Not enough stock for product {product.Name}. Available: {product.StockQuantity}, Requested: {item.Quantity}");

            product.UpdateStockQuantity(product.StockQuantity - item.Quantity);
            productsToUpdate.Add(product);

            totalAmount += item.Quantity * product.Price;
            itemsData.Add((item.ProductId, item.Quantity, product.Price));
        }

        var order = Order.Create(userId, totalAmount, OrderStatus.New);

        foreach (var data in itemsData)
        {
            var orderItem = OrderItem.Create(Guid.Empty, data.ProductId, data.Quantity, data.UnitPrice);
            order.OrderItems.Add(orderItem); 
        }

        foreach (var product in productsToUpdate)
        {
            _unitOfWork.Products.Update(product);
        }

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    public async Task UpdateStatusAsync(UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIDAsync(request.Id)
            ?? throw new KeyNotFoundException($"Order with Id {request.Id} not found");

        bool hasChanges = false;

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var newStatus = Enum.Parse<OrderStatus>(request.Status, true);

            if (order.Status != newStatus)
            {
                order.UpdateStatus(newStatus);
                hasChanges = true;
            }
        }

        if (!hasChanges) return;

        _unitOfWork.Orders.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIDAsync(orderId)
            ?? throw new KeyNotFoundException($"Order with Id {orderId} not found");

        _unitOfWork.Orders.Delete(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId, cancellationToken);
        return order == null ? null : _mapper.Map<OrderResponse>(order);
    }

    public async Task<List<OrderResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();
        return _mapper.Map<List<OrderResponse>>(orders);
    }

    public async Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<List<OrderResponse>>(orders);
    }
}