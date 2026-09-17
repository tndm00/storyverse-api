namespace Content.Application.Commands.Stories.QuickPublishStory;

public sealed class QuickPublishStoryCommandValidator : AbstractValidator<QuickPublishStoryCommand>
{
    /// <summary>Validates story and chapter fields together with genre (exactly one primary, no duplicates) and tag limits.</summary>
    public QuickPublishStoryCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(ApplicationConstants.MaxUrlLength);

        RuleFor(x => x.OriginalSource)
            .NotEmpty()
            .When(x => x.ContentType == StoryContentType.Translated)
            .WithMessage(ApplicationErrorConstants.OriginalSourceRequired);

        RuleFor(x => x.ChapterTitle)
            .MaximumLength(ApplicationConstants.MaxChapterTitleLength)
            .When(x => !string.IsNullOrWhiteSpace(x.ChapterTitle));

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

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= ApplicationConstants.MaxTagsPerStory)
            .WithMessage(ApplicationErrorConstants.TooManyTags);

        RuleForEach(x => x.Tags)
            .MaximumLength(ApplicationConstants.MaxTagNameLength);
    }
}
