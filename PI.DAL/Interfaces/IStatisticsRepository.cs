using PI.DAL.Models.Statistic;

namespace PI.DAL.Interfaces;

public interface IStatisticsRepository
{
    Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<int> GetTotalOrdersAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopProductModel>> GetTopSellingProductsAsync(int limit, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryRevenueModel>> GetRevenueByCategoryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<LowStockModel>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopUserModel>> GetTopUsersAsync(int limit, CancellationToken cancellationToken = default);
}