using FluentValidation;
using PI.BLL.DTOs.Orders;

namespace PI.BLL.Validators.Order;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("The order must contain at least one item.");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemValidator());
    }
}