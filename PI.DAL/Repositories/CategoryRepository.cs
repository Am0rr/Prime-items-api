using Microsoft.EntityFrameworkCore;
using PI.DAL.Entities.Catalog;
using PI.DAL.Interfaces;
using PI.DAL.Persistence;

namespace PI.DAL.Repositories;

public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(c => c.Name == name, cancellationToken);
    }
}