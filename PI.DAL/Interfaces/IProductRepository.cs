using PI.DAL.Entities.Catalog;
using PI.DAL.Models.Catalog;

namespace PI.DAL.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<ProductPagedResult> GetFilteredPagedAsync(ProductFilterModel filter, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}