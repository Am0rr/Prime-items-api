using FluentValidation;
using PI.BLL.DTOs.Identity;

namespace PI.BLL.Validators.Identity;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage("Email address cannot be empty.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage("Password cannot be empty")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(150).WithMessage("Password must not exceed 150 characters");
    }
}