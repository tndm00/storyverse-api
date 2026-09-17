namespace Authentication.Application.Commands.Logout;

/// <summary>Validation rules for <see cref="LogoutCommand"/>.</summary>
public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>Requires a non-empty refresh token.</summary>
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
