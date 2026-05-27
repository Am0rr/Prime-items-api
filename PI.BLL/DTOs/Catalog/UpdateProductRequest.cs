namespace PI.BLL.DTOs.Catalog;

public record UpdateProductRequest(
    Guid Id,
    Guid? CategoryId,
    string? Name,
    string? Description,
    decimal? Price,
    int? StockQuantity,
    string? ImageUrl
);