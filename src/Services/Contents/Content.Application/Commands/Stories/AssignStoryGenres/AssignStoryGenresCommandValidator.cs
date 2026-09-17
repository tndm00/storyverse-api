namespace Content.Application.Commands.Stories.AssignStoryGenres;

public sealed class AssignStoryGenresCommandValidator : AbstractValidator<AssignStoryGenresCommand>
{
    /// <summary>Validates the genre selection list: non-empty, exactly one primary, no duplicates, no blank slugs.</summary>
    public AssignStoryGenresCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.Genres)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ExactlyOnePrimaryGenreRequired);

        // Exactly one genre must be marked primary.
        RuleFor(x => x.Genres)
            .Must(genres => genres.Count(g => g.IsPrimary) == 1)
            .WithMessage(ApplicationErrorConstants.ExactlyOnePrimaryGenreRequired)
            .When(x => x.Genres.Count > 0);

        // No duplicate genre slugs in the selection.
        RuleFor(x => x.Genres)
            .Must(genres => genres
                .Select(g => (g.GenreSlug ?? string.Empty).Trim().ToLowerInvariant())
                .Distinct()
                .Count() == genres.Count)
            .WithMessage(ApplicationErrorConstants.DuplicateGenreSelection)
            .When(x => x.Genres.Count > 0);

        // Every selection must carry a non-blank slug.
        RuleForEach(x => x.Genres)
            .Must(g => !string.IsNullOrWhiteSpace(g.GenreSlug))
            .WithMessage(ApplicationErrorConstants.GenreInactiveOrMissing);
    }
}
