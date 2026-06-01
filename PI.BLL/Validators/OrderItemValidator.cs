using FluentValidation;
using PI.BLL.DTOs.Orders;

namespace PI.BLL.Validators.Order;

public class OrderItemValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemValidator()
    {
        RuleFor(i => i.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(i => i.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be strictly greater than 0.");
    }
}