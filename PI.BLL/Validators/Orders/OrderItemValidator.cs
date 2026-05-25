using FluentValidation;
using PI.BLL.DTOs.Orders;

namespace PI.BLL.Validators.Order;

public class OrderItemValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID must not be empty.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero. You cannot order 0 or negative items.");
    }
}