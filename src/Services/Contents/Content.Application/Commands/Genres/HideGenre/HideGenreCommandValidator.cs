namespace Content.Application.Commands.Genres.HideGenre;

public sealed class HideGenreCommandValidator : AbstractValidator<HideGenreCommand>
{
    public HideGenreCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
    }
}
