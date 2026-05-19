using PI.DAL.Interfaces;

namespace PI.DAL.Persistence;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;

    public IUserRepository Users { get; }
    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public IRefreshTokenRepository RefreshTokens { get; }

    public UnitOfWork(AppDbContext context,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _context = context;

        Users = userRepository;
        Categories = categoryRepository;
        Products = productRepository;
        Orders = orderRepository;
        RefreshTokens = refreshTokenRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}