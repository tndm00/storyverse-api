namespace Content.Application.Commands.Stories.GuestPublishStory;

public sealed class GuestPublishStoryCommandValidator : AbstractValidator<GuestPublishStoryCommand>
{
    public GuestPublishStoryCommandValidator()
    {
        RuleFor(x => x.GuestPenName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength);

        RuleFor(x => x.ChapterContent)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ContentRequired);

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
