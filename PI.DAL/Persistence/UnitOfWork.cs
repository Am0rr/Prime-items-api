using PI.DAL.Interfaces;
using PI.DAL.Repositories;

namespace PI.DAL.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; private set; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;

        Users = new UserRepository(_context);
        Categories = new CategoryRepository(_context);
        Products = new ProductRepository(_context);
        Orders = new OrderRepository(_context);
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}