using AutoMapper;
using PI.BLL.DTOs.Statistic;
using PI.BLL.Interfaces;
using PI.DAL.Interfaces;

namespace PI.BLL.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IStatisticsRepository _statisticsRepository;
    private readonly IMapper _mapper;

    public StatisticsService(
        IStatisticsRepository statisticsRepository,
        IMapper mapper)
    {
        _statisticsRepository = statisticsRepository;
        _mapper = mapper;
    }

    public async Task<SummaryResponse> GetSummaryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("The evaluation start date must precede or equal the end date.");
        }

        var totalRevenue = await _statisticsRepository.GetTotalRevenueAsync(startDate, endDate, cancellationToken);
        var totalOrders = await _statisticsRepository.GetTotalOrdersAsync(startDate, endDate, cancellationToken);

        return new SummaryResponse(totalRevenue, totalOrders);
    }

    public async Task<IEnumerable<TopProductResponse>> GetTopSellingProductsAsync(int limit, CancellationToken cancellationToken = default)
    {
        if (limit < 1 || limit > 100)
        {
            throw new ArgumentException("The extraction limit must fall within the 1-100 margin.", nameof(limit));
        }

        var products = await _statisticsRepository.GetTopSellingProductsAsync(limit, cancellationToken);

        return _mapper.Map<IEnumerable<TopProductResponse>>(products);
    }

    public async Task<IEnumerable<CategoryRevenueResponse>> GetRevenueByCategoryAsync(CancellationToken cancellationToken = default)
    {
        var revenues = await _statisticsRepository.GetRevenueByCategoryAsync(cancellationToken);

        return _mapper.Map<IEnumerable<CategoryRevenueResponse>>(revenues);
    }

    public async Task<IEnumerable<LowStockResponse>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        if (threshold < 0)
        {
            throw new ArgumentException("The minimum quantity threshold cannot be negative.", nameof(threshold));
        }

        var products = await _statisticsRepository.GetLowStockProductsAsync(threshold, cancellationToken);

        return _mapper.Map<IEnumerable<LowStockResponse>>(products);
    }

    public async Task<IEnumerable<TopUserResponse>> GetTopUsersAsync(int limit, CancellationToken cancellationToken = default)
    {
        if (limit < 1 || limit > 100)
        {
            throw new ArgumentException("The extraction limit must fall within the 1-100 margin.", nameof(limit));
        }

        var users = await _statisticsRepository.GetTopUsersAsync(limit, cancellationToken);

        return _mapper.Map<IEnumerable<TopUserResponse>>(users);
    }
}