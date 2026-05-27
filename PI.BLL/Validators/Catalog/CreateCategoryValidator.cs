using FluentValidation;
using PI.BLL.DTOs.Catalog;

namespace PI.BLL.Validators.Category;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Category name cannot be empty.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Category description cannot be empty.")
            .MaximumLength(1000).WithMessage("Category description must not exceed 1000 characters.");
    }
}