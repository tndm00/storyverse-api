namespace Content.Application.Dtos;

/// <summary>Payload to create a story (author only). The story starts as a draft.</summary>
public sealed class CreateStoryRequestDto
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string CoverImageUrl { get; init; }

    public StoryContentType ContentType { get; init; } = StoryContentType.Original;

    public string OriginalSource { get; init; }

    public string Language { get; init; } = ApplicationConstants.DefaultLanguage;

    public AgeRating AgeRating { get; init; } = AgeRating.General;
}

/// <summary>Editable story metadata. Does not change status or classification.</summary>
public sealed class UpdateStoryRequestDto
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string CoverImageUrl { get; init; }

    public StoryContentType ContentType { get; init; } = StoryContentType.Original;

    public string OriginalSource { get; init; }

    public string Language { get; init; } = ApplicationConstants.DefaultLanguage;

    public AgeRating AgeRating { get; init; } = AgeRating.General;
}

/// <summary>
/// One-call publish: creates the story, assigns genres/tags, writes the first
/// chapter, and publishes it. For a one-shot set <see cref="CompleteImmediately"/>.
/// </summary>
public sealed class QuickPublishStoryRequestDto
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string CoverImageUrl { get; init; }

    public StoryContentType ContentType { get; init; } = StoryContentType.Original;

    public string OriginalSource { get; init; }

    public string Language { get; init; } = ApplicationConstants.DefaultLanguage;

    public AgeRating AgeRating { get; init; } = AgeRating.General;

    public IReadOnlyList<StoryGenreSelectionDto> Genres { get; init; } = Array.Empty<StoryGenreSelectionDto>();

    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Optional. Blank falls back to the story title (one-shot ergonomics).</summary>
    public string ChapterTitle { get; init; }

    public string ChapterContent { get; init; } = string.Empty;

    /// <summary>When true the story is marked <c>Completed</c> right after the first chapter publishes.</summary>
    public bool CompleteImmediately { get; init; }
}

/// <summary>Full replacement of a story's genre set. Exactly one item must be primary.</summary>
public sealed class AssignStoryGenresRequestDto
{
    public IReadOnlyList<StoryGenreSelectionDto> Genres { get; init; } = Array.Empty<StoryGenreSelectionDto>();
}

public sealed class StoryGenreSelectionDto
{
    public string GenreSlug { get; init; } = string.Empty;

    public bool IsPrimary { get; init; }
}

/// <summary>
/// Anonymous one-call publish: no account, the author types a pen name. Creates the
/// story (owner id 0), its first published chapter, and marks the story Ongoing.
/// </summary>
public sealed class GuestPublishStoryRequestDto
{
    public string GuestPenName { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<StoryGenreSelectionDto> Genres { get; init; } = Array.Empty<StoryGenreSelectionDto>();

    public string ChapterContent { get; init; } = string.Empty;
}

/// <summary>Full replacement of a story's free-form tag set (tag display names).</summary>
public sealed class AssignStoryTagsRequestDto
{
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>Requests a story lifecycle transition (Ongoing, Hiatus, Completed, Dropped).</summary>
public sealed class ChangeStoryStatusRequestDto
{
    public StoryStatus TargetStatus { get; init; }
}
