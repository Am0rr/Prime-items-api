namespace PI.BLL.DTOs.Catalog;

public record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string? ImageUrl
);