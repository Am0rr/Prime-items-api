using PI.DAL.Entities;

namespace PI.DAL.Interfaces;

public interface IBaseRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(T item);
    void Update(T item);
    void Delete(T item);
}