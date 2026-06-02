using PI.BLL.DTOs.Catalog;
using PI.DAL.Models.Catalog;

namespace PI.BLL.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductPagedResponse> GetPagedAsync(ProductFilterModel filter, CancellationToken cancellationToken = default);
}