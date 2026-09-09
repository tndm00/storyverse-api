namespace Content.Application.Commands.Stories.AssignStoryGenres;

public sealed class AssignStoryGenresCommandValidator : AbstractValidator<AssignStoryGenresCommand>
{
    public AssignStoryGenresCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.Genres)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ExactlyOnePrimaryGenreRequired);

        RuleFor(x => x.Genres)
            .Must(genres => genres.Count(g => g.IsPrimary) == 1)
            .WithMessage(ApplicationErrorConstants.ExactlyOnePrimaryGenreRequired)
            .When(x => x.Genres.Count > 0);

        RuleFor(x => x.Genres)
            .Must(genres => genres
                .Select(g => (g.GenreSlug ?? string.Empty).Trim().ToLowerInvariant())
                .Distinct()
                .Count() == genres.Count)
            .WithMessage(ApplicationErrorConstants.DuplicateGenreSelection)
            .When(x => x.Genres.Count > 0);

        RuleForEach(x => x.Genres)
            .Must(g => !string.IsNullOrWhiteSpace(g.GenreSlug))
            .WithMessage(ApplicationErrorConstants.GenreInactiveOrMissing);
    }
}
