using FluentValidation;
using PI.BLL.DTOs.Identity;

namespace PI.BLL.Validators.Identity;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username cannot be empty.")
            .MaximumLength(50).WithMessage("Username must not exceed 100 characters");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email address cannot be empty.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(u => u.Password)
            .NotEmpty().WithMessage("Password cannot be empty")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(150).WithMessage("Password must not exceed 150 characters");
    }
}