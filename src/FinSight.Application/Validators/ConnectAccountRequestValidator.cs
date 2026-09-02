using FinSight.Application.Features.Accounts;
using FluentValidation;

namespace FinSight.Application.Validators;

/// <summary>
/// Validates financial institution connection requests.
/// </summary>
public sealed class ConnectAccountRequestValidator
    : AbstractValidator<ConnectAccountRequest>
{
    /// <summary>
    /// Initializes the validator.
    /// </summary>
    public ConnectAccountRequestValidator()
    {
        RuleFor(x => x.InstitutionCode)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9_-]+$")
            .WithMessage(
                "Institution code contains invalid characters.");
    }
}
