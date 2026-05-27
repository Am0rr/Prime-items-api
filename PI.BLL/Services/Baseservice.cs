using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TL.BLL.Services;

public class BaseService
{
    protected readonly IServiceProvider _serviceProvider;

    protected BaseService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected async Task ValidateAsync<TRequest>(TRequest request)
    {
        var validator = _serviceProvider.GetService<IValidator<TRequest>>();

        if (validator != null)
        {
            await validator.ValidateAndThrowAsync(request);
        }
    }
}