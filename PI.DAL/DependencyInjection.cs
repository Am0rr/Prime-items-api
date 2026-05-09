using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PI.DAL.Persistence;
using Microsoft.EntityFrameworkCore;
using PI.DAL.Interfaces;

namespace PI.DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}