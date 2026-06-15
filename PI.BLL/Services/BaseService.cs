using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace PI.BLL.Services;

public class BaseService
{
    protected readonly IServiceProvider _serviceProvider;

    protected BaseService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected void Validate<TRequest>(TRequest request)
    {
        var validator = _serviceProvider.GetService<IValidator<TRequest>>();

        if (validator != null)
        {
            validator.ValidateAndThrow(request);
        }
    }
}