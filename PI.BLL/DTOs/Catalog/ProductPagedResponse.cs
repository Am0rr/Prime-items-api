namespace PI.BLL.DTOs.Catalog;

public record ProductPagedResponse(
    IEnumerable<ProductResponse> Items,
    int TotalCount
);