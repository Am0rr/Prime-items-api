namespace PI.BLL.DTOs.Catalog;

public record UpdateCategoryRequest(
    string? Name,
    string? Description
);