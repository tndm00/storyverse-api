namespace Content.Application.Commands.Stories.QuickPublishStory;

/// <summary>
/// Creates a story, assigns its genres/tags, writes the first chapter, and
/// publishes it in one atomic operation (Author Onboarding / Story Setup flow,
/// product-workflow-context.md sections 5.1-5.3). Owner is the calling author.
/// Set <see cref="CompleteImmediately"/> for a one-shot.
/// </summary>
public sealed class QuickPublishStoryCommand : ICommand<QuickPublishStoryResultDto>
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string CoverImageUrl { get; init; }

    public StoryContentType ContentType { get; init; } = StoryContentType.Original;

    public string OriginalSource { get; init; }

    public string Language { get; init; } = ApplicationConstants.DefaultLanguage;

    public AgeRating AgeRating { get; init; } = AgeRating.General;

    public IReadOnlyList<StoryGenreSelection> Genres { get; init; } = Array.Empty<StoryGenreSelection>();

    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Optional; blank falls back to <see cref="Title"/>.</summary>
    public string ChapterTitle { get; init; }

    public string ChapterContent { get; init; } = string.Empty;

    public bool CompleteImmediately { get; init; }
}
