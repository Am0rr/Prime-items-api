using AutoMapper;
using Moq;
using PI.BLL.DTOs.Orders;
using PI.BLL.Exceptions;
using PI.BLL.Services;
using PI.DAL.Entities.Catalog;
using PI.DAL.Entities.Orders;
using PI.DAL.Enums;
using PI.DAL.Interfaces;
using Xunit;

namespace PI.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _mapperMock = new Mock<IMapper>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _sut = new OrderService(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _serviceProviderMock.Object);
    }

    private static Product CreateProduct(int stockQuantity, decimal price = 10m) =>
        Product.Create(Guid.NewGuid(), "Test product", "Test description", price, stockQuantity, null);

    [Fact]
    public async Task CreateAsync_WithValidData_ReducesStockAddsOrderAndReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 5, price: 20m);
        var request = new CreateOrderRequest(new List<OrderItemRequest>
        {
            new OrderItemRequest(productId, 2)
        });
        var expectedResponse = new OrderResponse { Id = Guid.NewGuid(), UserId = userId };

        _productRepoMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _mapperMock
            .Setup(m => m.Map<OrderResponse>(It.IsAny<Order>()))
            .Returns(expectedResponse);

        var result = await _sut.CreateAsync(request, userId, CancellationToken.None);

        Assert.Equal(3, product.StockQuantity);
        Assert.Same(expectedResponse, result);
        _orderRepoMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenProductNotFound_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();
        var request = new CreateOrderRequest(new List<OrderItemRequest>
        {
            new OrderItemRequest(productId, 1)
        });

        _productRepoMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateAsync(request, Guid.NewGuid(), CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenStockIsInsufficient_ThrowsInvalidOperationException()
    {
        var productId = Guid.NewGuid();
        var product = CreateProduct(stockQuantity: 1);
        var request = new CreateOrderRequest(new List<OrderItemRequest>
        {
            new OrderItemRequest(productId, 2)
        });

        _productRepoMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CreateAsync(request, Guid.NewGuid(), CancellationToken.None));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_WithExistingOrder_ChangesStatusAndSaves()
    {
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid());
        var request = new UpdateOrderStatusRequest(nameof(OrderStatus.Paid));

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _sut.UpdateStatusAsync(orderId, request, CancellationToken.None);

        Assert.Equal(OrderStatus.Paid, order.Status);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenOrderNotFound_ThrowsKeyNotFoundException()
    {
        var orderId = Guid.NewGuid();
        var request = new UpdateOrderStatusRequest(nameof(OrderStatus.Paid));

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateStatusAsync(orderId, request, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingOrder_DeletesAndSaves()
    {
        var orderId = Guid.NewGuid();
        var order = Order.Create(Guid.NewGuid());

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _sut.DeleteAsync(orderId, CancellationToken.None);

        _orderRepoMock.Verify(r => r.Delete(order), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenOrderNotFound_ThrowsKeyNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.DeleteAsync(orderId, CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwnerRequestsOwnOrder_ReturnsResponse()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = Order.Create(userId);
        var expectedResponse = new OrderResponse { Id = orderId, UserId = userId };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mapperMock
            .Setup(m => m.Map<OrderResponse>(order))
            .Returns(expectedResponse);

        var result = await _sut.GetByIdAsync(orderId, userId, nameof(UserRole.Registered), CancellationToken.None);

        Assert.Same(expectedResponse, result);
    }

    [Theory]
    [InlineData(nameof(UserRole.Admin))]
    [InlineData(nameof(UserRole.Manager))]
    public async Task GetByIdAsync_WhenPrivilegedRole_CanAccessOtherUsersOrder(string role)
    {
        var ownerId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = Order.Create(ownerId);
        var expectedResponse = new OrderResponse { Id = orderId, UserId = ownerId };

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        _mapperMock
            .Setup(m => m.Map<OrderResponse>(order))
            .Returns(expectedResponse);

        var result = await _sut.GetByIdAsync(orderId, callerId, role, CancellationToken.None);

        Assert.Same(expectedResponse, result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenRegisteredAccessesOtherUsersOrder_ThrowsForbiddenException()
    {
        var ownerId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = Order.Create(ownerId);

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.GetByIdAsync(orderId, callerId, nameof(UserRole.Registered), CancellationToken.None));
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderNotFound_ThrowsKeyNotFoundException()
    {
        var orderId = Guid.NewGuid();
        _orderRepoMock
            .Setup(r => r.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetByIdAsync(orderId, Guid.NewGuid(), nameof(UserRole.Registered), CancellationToken.None));
    }

    [Fact]
    public async Task GetUserOrdersAsync_WhenOwnerRequestsOwnOrders_ReturnsResponses()
    {
        var userId = Guid.NewGuid();
        var orders = new List<Order> { Order.Create(userId) };
        var expected = new List<OrderResponse> { new OrderResponse { UserId = userId } };

        _orderRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);
        _mapperMock
            .Setup(m => m.Map<IEnumerable<OrderResponse>>(orders))
            .Returns(expected);

        var result = await _sut.GetUserOrdersAsync(userId, userId, nameof(UserRole.Registered), CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Theory]
    [InlineData(nameof(UserRole.Admin))]
    [InlineData(nameof(UserRole.Manager))]
    public async Task GetUserOrdersAsync_WhenPrivilegedRole_CanAccessOtherUsersOrders(string role)
    {
        var targetUserId = Guid.NewGuid();
        var callerId = Guid.NewGuid();
        var orders = new List<Order> { Order.Create(targetUserId) };
        var expected = new List<OrderResponse> { new OrderResponse { UserId = targetUserId } };

        _orderRepoMock
            .Setup(r => r.GetByUserIdAsync(targetUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);
        _mapperMock
            .Setup(m => m.Map<IEnumerable<OrderResponse>>(orders))
            .Returns(expected);

        var result = await _sut.GetUserOrdersAsync(targetUserId, callerId, role, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetUserOrdersAsync_WhenRegisteredRequestsOtherUsersOrders_ThrowsForbiddenException()
    {
        var targetUserId = Guid.NewGuid();
        var callerId = Guid.NewGuid();

        await Assert.ThrowsAsync<ForbiddenException>(
            () => _sut.GetUserOrdersAsync(targetUserId, callerId, nameof(UserRole.Registered), CancellationToken.None));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedResponses()
    {
        var orders = new List<Order>
        {
            Order.Create(Guid.NewGuid()),
            Order.Create(Guid.NewGuid())
        };
        var expected = new List<OrderResponse>
        {
            new OrderResponse(),
            new OrderResponse()
        };

        _orderRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);
        _mapperMock
            .Setup(m => m.Map<IEnumerable<OrderResponse>>(orders))
            .Returns(expected);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        Assert.Same(expected, result);
        _mapperMock.Verify(m => m.Map<IEnumerable<OrderResponse>>(orders), Times.Once);
    }
}