using PI.DAL.Entities.Catalog;
using PI.DAL.Models.Catalog;

namespace PI.DAL.Interfaces;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetFilteredPagedAsync(ProductFilterModel filter, CancellationToken cancellationToken = default);
    Task<Product?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}