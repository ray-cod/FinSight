using FinSight.Application.Abstractions.Identity;
using FluentValidation;

namespace FinSight.Application.Validators;

/// <summary>
/// Validates login requests.
/// </summary>
public sealed class LoginRequestValidator
    : AbstractValidator<LoginRequest>
{
    /// <summary>
    /// Initializes the validator.
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
