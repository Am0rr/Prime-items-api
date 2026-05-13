using PI.BLL.DTOs.Catalog;
using PI.DAL.Models;

namespace PI.BLL.Interfaces;

public interface IProductService
{
    Task<Guid> CreateAsync(CreateProductRequest request);
    Task UpdateAsync(UpdateProductRequest request);
    Task DeleteAsync(Guid id);
    Task<ProductResponse?> GetByIdAsync(Guid id);
    Task<List<ProductResponse>> GetAllAsync();
    Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetPagedAsync(ProductFilterParams filter);
}