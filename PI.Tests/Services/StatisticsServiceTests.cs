using AutoMapper;
using Moq;
using PI.BLL.DTOs.Statistic;
using PI.BLL.Services;
using PI.DAL.Interfaces;
using PI.DAL.Models.Statistic;

namespace PI.Tests.Services;

public class StatisticsServiceTests
{
    private readonly Mock<IStatisticsRepository> _statisticsRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly StatisticsService _statisticsService;

    public StatisticsServiceTests()
    {
        _statisticsRepositoryMock = new Mock<IStatisticsRepository>();
        _mapperMock = new Mock<IMapper>();

        _statisticsService = new StatisticsService(
            _statisticsRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task GetSummaryAsync_SuccessPath_ReturnsSummaryResponse()
    {
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 12, 31);
        var expectedRevenue = 15000.50m;
        var expectedOrders = 120;

        _statisticsRepositoryMock.Setup(r => r.GetTotalRevenueAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRevenue);
        _statisticsRepositoryMock.Setup(r => r.GetTotalOrdersAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        var result = await _statisticsService.GetSummaryAsync(startDate, endDate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedRevenue, result.TotalRevenue);
        Assert.Equal(expectedOrders, result.TotalOrders);

        _statisticsRepositoryMock.Verify(r => r.GetTotalRevenueAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
        _statisticsRepositoryMock.Verify(r => r.GetTotalOrdersAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSummaryAsync_FailInvalidDates_ThrowsArgumentException()
    {
        var startDate = new DateTime(2026, 12, 31);
        var endDate = new DateTime(2026, 1, 1);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _statisticsService.GetSummaryAsync(startDate, endDate, CancellationToken.None));

        _statisticsRepositoryMock.Verify(r => r.GetTotalRevenueAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        _statisticsRepositoryMock.Verify(r => r.GetTotalOrdersAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTopSellingProductsAsync_SuccessPath_ReturnsMappedCollection()
    {
        var limit = 10;
        var dalModels = new List<TopProductModel>
        {
            new TopProductModel { ProductId = Guid.NewGuid(), Name = "Product A", TotalItemsSold = 50, TotalRevenue = 500m }
        };
        var expectedResponses = new List<TopProductResponse>
        {
            new TopProductResponse(dalModels[0].ProductId, "Product A", 50, 500m)
        };

        _statisticsRepositoryMock.Setup(r => r.GetTopSellingProductsAsync(limit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dalModels);
        _mapperMock.Setup(m => m.Map<IEnumerable<TopProductResponse>>(dalModels))
            .Returns(expectedResponses);

        var result = await _statisticsService.GetTopSellingProductsAsync(limit, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        _statisticsRepositoryMock.Verify(r => r.GetTopSellingProductsAsync(limit, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTopSellingProductsAsync_FailLimitBelowOne_ThrowsArgumentException(int limit)
    {
        await Assert.ThrowsAsync<ArgumentException>("limit", () =>
            _statisticsService.GetTopSellingProductsAsync(limit, CancellationToken.None));

        _statisticsRepositoryMock.Verify(r => r.GetTopSellingProductsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTopSellingProductsAsync_FailLimitOverOneHundred_ThrowsArgumentException()
    {
        var limit = 101;

        await Assert.ThrowsAsync<ArgumentException>("limit", () =>
            _statisticsService.GetTopSellingProductsAsync(limit, CancellationToken.None));

        _statisticsRepositoryMock.Verify(r => r.GetTopSellingProductsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetRevenueByCategoryAsync_SuccessPath_ReturnsMappedCollection()
    {
        var dalModels = new List<CategoryRevenueModel>
        {
            new CategoryRevenueModel { CategoryName = "Electronics", TotalRevenue = 12000m }
        };
        var expectedResponses = new List<CategoryRevenueResponse>
        {
            new CategoryRevenueResponse("Electronics", 12000m)
        };

        _statisticsRepositoryMock.Setup(r => r.GetRevenueByCategoryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dalModels);
        _mapperMock.Setup(m => m.Map<IEnumerable<CategoryRevenueResponse>>(dalModels))
            .Returns(expectedResponses);

        var result = await _statisticsService.GetRevenueByCategoryAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        _statisticsRepositoryMock.Verify(r => r.GetRevenueByCategoryAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public async Task GetLowStockProductsAsync_SuccessPath_ReturnsMappedCollection(int threshold)
    {
        var dalModels = new List<LowStockModel>
        {
            new LowStockModel { ProductId = Guid.NewGuid(), Name = "Scarce Item", StockQuantity = threshold }
        };
        var expectedResponses = new List<LowStockResponse>
        {
            new LowStockResponse(dalModels[0].ProductId, "Scarce Item", threshold)
        };

        _statisticsRepositoryMock.Setup(r => r.GetLowStockProductsAsync(threshold, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dalModels);
        _mapperMock.Setup(m => m.Map<IEnumerable<LowStockResponse>>(dalModels))
            .Returns(expectedResponses);

        var result = await _statisticsService.GetLowStockProductsAsync(threshold, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        _statisticsRepositoryMock.Verify(r => r.GetLowStockProductsAsync(threshold, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetLowStockProductsAsync_FailNegativeThreshold_ThrowsArgumentException()
    {
        var threshold = -1;

        await Assert.ThrowsAsync<ArgumentException>("threshold", () =>
            _statisticsService.GetLowStockProductsAsync(threshold, CancellationToken.None));

        _statisticsRepositoryMock.Verify(r => r.GetLowStockProductsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTopUsersAsync_SuccessPath_ReturnsMappedCollection()
    {
        var limit = 10;
        var dalModels = new List<TopUserModel>
        {
            new TopUserModel { UserId = Guid.NewGuid(), Email = "user@test.com", TotalSpent = 1500m, TotalOrders = 3 }
        };
        var expectedResponses = new List<TopUserResponse>
        {
            new TopUserResponse(dalModels[0].UserId, "user@test.com", 1500m, 3)
        };

        _statisticsRepositoryMock.Setup(r => r.GetTopUsersAsync(limit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dalModels);
        _mapperMock.Setup(m => m.Map<IEnumerable<TopUserResponse>>(dalModels))
            .Returns(expectedResponses);

        var result = await _statisticsService.GetTopUsersAsync(limit, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Single(result);
        _statisticsRepositoryMock.Verify(r => r.GetTopUsersAsync(limit, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetTopUsersAsync_FailLimitBelowOne_ThrowsArgumentException(int limit)
    {
        await Assert.ThrowsAsync<ArgumentException>("limit", () =>
            _statisticsService.GetTopUsersAsync(limit, CancellationToken.None));

        _statisticsRepositoryMock.Verify(r => r.GetTopUsersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTopUsersAsync_FailLimitOverOneHundred_ThrowsArgumentException()
    {
        var limit = 101;

        await Assert.ThrowsAsync<ArgumentException>("limit", () =>
            _statisticsService.GetTopUsersAsync(limit, CancellationToken.None));

        _statisticsRepositoryMock.Verify(r => r.GetTopUsersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}