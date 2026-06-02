using FluentValidation;
using PI.BLL.DTOs.Orders;
using PI.DAL.Enums;

namespace PI.BLL.Validators.Order;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status cannot be empty if provided.")
            .IsEnumName(typeof(OrderStatus), caseSensitive: false)
            .WithMessage("Invalid Status. Allowed: New, Paid, Delivered, Cancelled.")
            .When(x => x.Status != null);
    }
}