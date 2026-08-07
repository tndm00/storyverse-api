namespace Authentication.Application.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
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
