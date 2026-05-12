namespace PI.BLL.DTOs.Catalog;

public record CategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}