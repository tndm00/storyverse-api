namespace Content.Application.Commands.Genres.HideGenre;

public sealed class HideGenreCommandValidator : AbstractValidator<HideGenreCommand>
{
    /// <summary>Validates that a slug is provided.</summary>
    public HideGenreCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
    }
}
