namespace Authentication.Application.Commands.GoogleLogin;

/// <summary>Validation rules for <see cref="GoogleLoginCommand"/>.</summary>
public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    /// <summary>Requires a non-empty Google ID token.</summary>
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty();
    }
}
