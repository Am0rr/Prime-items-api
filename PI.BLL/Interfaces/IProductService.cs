using PI.BLL.DTOs.Catalog;
using PI.DAL.Models.Catalog;

namespace PI.BLL.Interfaces;

public interface IProductService
{
    Task<Guid> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<ProductResponse> Items, int TotalCount)> GetPagedAsync(ProductFilterModel filter, CancellationToken cancellationToken = default);
}