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

    public async Task<Guid> CreateAsync(CreateOrderRequest request, Guid userId)
    {
        decimal totalAmount = 0;
        var itemsData = new List<(Guid ProductId, int Quantity, decimal UnitPrice)>();

        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIDAsync(item.ProductId);
            if (product == null)
                throw new Exception($"Product with ID {item.ProductId} not found.");

            totalAmount += item.Quantity * product.Price;
            itemsData.Add((item.ProductId, item.Quantity, product.Price));
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = Order.Create(userId, totalAmount, OrderStatus.New);

            foreach (var data in itemsData)
            {
                var orderItem = OrderItem.Create(order.Id, data.ProductId, data.Quantity, data.UnitPrice);
                order.OrderItems.Add(orderItem); 
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CommitTransactionAsync();

            return order.Id;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task UpdateStatusAsync(UpdateOrderStatusRequest request)
    {
        var newStatus = Enum.Parse<OrderStatus>(request.Status, true);

        var order = await _unitOfWork.Orders.GetByIDAsync(request.Id);
        if (order == null)
            throw new Exception($"Order with ID {request.Id} not found.");

        if (order.Status == newStatus)
            return;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            order.UpdateStatus(newStatus);
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Guid orderId)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var order = await _unitOfWork.Orders.GetByIDAsync(orderId);
            if (order != null)
            {
                _unitOfWork.Orders.Delete(order);
                await _unitOfWork.CommitTransactionAsync();
            }
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid orderId)
    {
        var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
        return order == null ? null : _mapper.Map<OrderResponse>(order);
    }

    public async Task<List<OrderResponse>> GetAllAsync()
    {
        var orders = await _unitOfWork.Orders.GetAllAsync();
        return _mapper.Map<List<OrderResponse>>(orders);
    }

    public async Task<List<OrderResponse>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
        return _mapper.Map<List<OrderResponse>>(orders);
    }
}