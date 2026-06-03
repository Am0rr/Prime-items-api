using Microsoft.EntityFrameworkCore;
using PI.DAL.Interfaces;
using PI.DAL.Models.Statistic;
using PI.DAL.Persistence;

namespace PI.DAL.Repositories;

public class StatisticsRepository : IStatisticsRepository
{
    private readonly AppDbContext _context;

    public StatisticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .SumAsync(o => o.TotalAmount, cancellationToken);
    }

    public async Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopProductModel>> GetTopSellingProductsAsync(int limit, CancellationToken cancellationToken)
    {
        return await _context.OrderItems
            .AsNoTracking()
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new TopProductModel
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                TotalItemsSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
            })
            .OrderByDescending(x => x.TotalItemsSold)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryRevenueModel>> GetRevenueByCategoryAsync(CancellationToken cancellationToken)
    {
        return await _context.OrderItems
            .AsNoTracking()
            .GroupBy(oi => oi.Product.Category.Name)
            .Select(g => new CategoryRevenueModel
            {
                CategoryName = g.Key,
                TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<LowStockModel>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.StockQuantity < threshold)
            .Select(p => new LowStockModel
            {
                ProductId = p.Id,
                Name = p.Name,
                StockQuantity = p.StockQuantity
            })
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TopUserModel>> GetTopUsersAsync(int limit, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .AsNoTracking()
            .GroupBy(o => new { o.UserId, o.User.Email })
            .Select(g => new TopUserModel
            {
                UserId = g.Key.UserId,
                Email = g.Key.Email,
                TotalSpent = g.Sum(o => o.TotalAmount),
                TotalOrders = g.Count()
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}