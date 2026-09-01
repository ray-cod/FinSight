using FinSight.Application.Abstractions.Identity;
using FluentValidation;

namespace FinSight.Application.Validators;

/// <summary>
/// Validates user registration requests.
/// </summary>
public sealed class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// Initializes the validator.
    /// </summary>
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .Matches("[A-Z]")
            .WithMessage(
                "Password must contain an uppercase letter.")
            .Matches("[a-z]")
            .WithMessage(
                "Password must contain a lowercase letter.")
            .Matches("[0-9]")
            .WithMessage(
                "Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage(
                "Password must contain a special character.");
    }
}
