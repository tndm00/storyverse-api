namespace Authentication.Application.Commands.Refresh;

/// <summary>Validation rules for <see cref="RefreshCommand"/>.</summary>
public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    /// <summary>Requires a non-empty refresh token.</summary>
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
