using PI.DAL.Entities.Catalog;

namespace PI.DAL.Interfaces;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}