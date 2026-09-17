namespace Authentication.Application.Commands.Login;

/// <summary>Validation rules for <see cref="LoginCommand"/>.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Requires a valid email format and a non-empty password.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(email => email.IsValidEmail())
            .WithMessage(ApplicationErrorConstants.InvalidEmailFormat);

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}
