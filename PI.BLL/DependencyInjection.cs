using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using PI.BLL.Interfaces;
using PI.BLL.Services;
using PI.BLL.Validators.Identity;
using FluentValidation;


namespace PI.BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddValidatorsFromAssemblyContaining<CreateUserValidator>();

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}