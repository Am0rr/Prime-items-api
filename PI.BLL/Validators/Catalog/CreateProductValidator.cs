using FluentValidation;
using PI.BLL.DTOs.Catalog;

namespace PI.BLL.Validators.Product;

public class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name cannot be empty.")
            .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");

        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Product description cannot be empty.")
            .MaximumLength(1000).WithMessage("Product description must not exceed 1000 characters.");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Price must be strictly greater than 0.");

        RuleFor(p => p.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

        RuleFor(p => p.ImageUrl)
            .MaximumLength(255).WithMessage("Image URL must not exceed 255 characters.")
            .When(p => !string.IsNullOrEmpty(p.ImageUrl));
    }
}