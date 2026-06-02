using FluentValidation;
using PI.BLL.DTOs.Identity;
using PI.DAL.Enums;

namespace PI.BLL.Validators.Identity;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("Username cannot be empty.")
            .MaximumLength(50).WithMessage("Username must not exceed 100 characters")
            .When(u => u.Username != null); ;

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email address cannot be empty.")
            .EmailAddress().WithMessage("Email is not valid.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters")
            .When(u => u.Email != null);

        RuleFor(u => u.Role)
            .NotEmpty().WithMessage("Role cannot be empty.")
            .IsEnumName(typeof(UserRole), caseSensitive: false)
            .WithMessage("Selected role is invalid. Allowed values are: Registered, Admin, etc.")
            .When(u => u.Role != null);
    }
}