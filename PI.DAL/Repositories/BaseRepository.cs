using Microsoft.EntityFrameworkCore;
using PI.DAL.Entities;
using PI.DAL.Interfaces;
using PI.DAL.Persistence;

namespace PI.DAL.Repositories;

public abstract class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task<Guid> AddAsync(T item, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(item, cancellationToken);
        return item.Id;
    }

    public void Update(T item)
    {
        _dbSet.Update(item);
    }
    public void Delete(T item)
    {
        _dbSet.Remove(item);
    }
}

