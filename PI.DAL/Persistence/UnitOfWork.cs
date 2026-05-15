using Microsoft.EntityFrameworkCore.Storage;
using PI.DAL.Interfaces;

namespace PI.DAL.Persistence;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }

    public UnitOfWork(AppDbContext context,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository)
    {
        _context = context;

        Users = userRepository;
        Categories = categoryRepository;
        Products = productRepository;
        Orders = orderRepository;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);

            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        await _context.DisposeAsync();

        GC.SuppressFinalize(this);
    }
}