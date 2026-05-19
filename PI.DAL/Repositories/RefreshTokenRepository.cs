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

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
}