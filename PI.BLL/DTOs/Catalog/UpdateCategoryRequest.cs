namespace PI.BLL.DTOs.Catalog;

public record UpdateCategoryRequest(
    Guid Id,
    string? Name,
    string? Description
);