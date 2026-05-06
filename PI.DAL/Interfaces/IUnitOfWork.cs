namespace PI.DAL.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    Task CompleteAsync(CancellationToken cancellationToken = default);
}