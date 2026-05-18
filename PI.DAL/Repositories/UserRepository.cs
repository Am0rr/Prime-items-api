using Microsoft.EntityFrameworkCore;
using PI.DAL.Entities.Identity;
using PI.DAL.Interfaces;
using PI.DAL.Persistence;

namespace PI.DAL.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }
}