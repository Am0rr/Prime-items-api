namespace PI.BLL.DTOs.Statistic;

public record CategoryRevenueResponse(
    string CategoryName,
    decimal TotalRevenue
);

public record LowStockResponse(
    Guid ProductId,
    string Name,
    int StockQuantity
);

public record TopUserResponse(
    Guid UserId,
    string Email,
    decimal TotalSpent,
    int TotalOrders
);

public record TopProductResponse(
    Guid ProductId,
    string Name,
    int TotalItemsSold,
    decimal TotalRevenue
);

public record SummaryResponse(
    decimal TotalRevenue,
    int TotalOrders
);