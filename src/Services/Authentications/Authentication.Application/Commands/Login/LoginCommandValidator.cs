namespace Authentication.Application.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
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
