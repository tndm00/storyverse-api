namespace Authentication.Application.Commands.Register;

/// <summary>Validation rules for <see cref="RegisterCommand"/>.</summary>
public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    /// <summary>Requires a valid email, a strong password, and a non-empty display name.</summary>
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(email => email.IsValidEmail())
            .WithMessage(ApplicationErrorConstants.InvalidEmailFormat);

        RuleFor(x => x.Password)
            .NotEmpty()
            .Must(password => password.IsStrongPassword())
            .WithMessage(string.Format(
                ApplicationErrorConstants.PasswordStrengthRequirementFormat,
                ApplicationConstants.MinPasswordLength));

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
