using PI.BLL.DTOs.Catalog;

namespace PI.BLL.Interfaces;

public interface ICategoryService
{
    Task<Guid> CreateAsync(CreateCategoryRequest request);
    Task UpdateAsync(UpdateCategoryRequest request);
    Task DeleteAsync(Guid id);
    Task<CategoryResponse?> GetByIdAsync(Guid id);
    Task<List<CategoryResponse>> GetAllAsync();
}