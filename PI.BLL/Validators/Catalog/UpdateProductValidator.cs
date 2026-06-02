using FluentValidation;
using PI.BLL.DTOs.Catalog;

namespace PI.BLL.Validators.Product;

public class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("Category ID cannot be empty.")
            .When(p => p.CategoryId != null);

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.")
            .When(p => p.Name != null);

        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Product description cannot be empty.")
            .MaximumLength(1000).WithMessage("Product description must not exceed 1000 characters.")
            .When(p => p.Description != null);

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Price must be strictly greater than 0.")
            .When(p => p.Price != null);

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.")
            .When(p => p.StockQuantity != null);

        RuleFor(p => p.ImageUrl)
            .MaximumLength(255).WithMessage("Image URL must not exceed 255 characters.")
            .When(p => p.ImageUrl != null);
    }
}