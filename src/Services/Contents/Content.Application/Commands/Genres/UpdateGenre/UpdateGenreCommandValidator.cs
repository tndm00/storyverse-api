namespace Content.Application.Commands.Genres.UpdateGenre;

public sealed class UpdateGenreCommandValidator : AbstractValidator<UpdateGenreCommand>
{
    public UpdateGenreCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxGenreNameLength);

        // Negative values are allowed on purpose — the admin UI documents this
        // as a way to bump a genre to the very top of the list.
    }
}
