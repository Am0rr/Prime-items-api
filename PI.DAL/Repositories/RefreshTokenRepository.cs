using PI.DAL.Interfaces;
using PI.DAL.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using PI.DAL.Persistence;

namespace PI.DAL.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DbSet<RefreshToken> _dbSet;

    public RefreshTokenRepository(AppDbContext context)
    {
        _dbSet = context.Set<RefreshToken>();
    }

    public void Add(RefreshToken refreshToken)
    {
        _dbSet.Add(refreshToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
}