using PI.BLL.DTOs.Catalog;

namespace PI.BLL.Interfaces;

public interface ICategoryService
{
    Task<Guid> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}