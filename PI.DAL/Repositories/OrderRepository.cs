using Microsoft.EntityFrameworkCore;
using PI.DAL.Entities.Orders;
using PI.DAL.Interfaces;
using PI.DAL.Persistence;

namespace PI.DAL.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}