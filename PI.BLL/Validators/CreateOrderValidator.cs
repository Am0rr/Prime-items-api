using FluentValidation;
using PI.BLL.DTOs.Orders;

namespace PI.BLL.Validators.Order;

public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(o => o.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(o => o.Items)
            .SetValidator(new OrderItemValidator());
    }
}