namespace Content.Application.Dtos;

/// <summary>Story card shown in discovery/browse listings.</summary>
public sealed class StorySummaryResponseDto
{
    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Slug { get; init; }

    public string CoverImageUrl { get; init; }

    public string Status { get; init; }

    public string AgeRating { get; init; }

    public string PrimaryGenre { get; init; }

    public int ViewCount { get; init; }

    public decimal RatingAvg { get; init; }

    public int RatingCount { get; init; }

    public DateTime? PublishedAt { get; init; }
}

/// <summary>Full story detail (story page header).</summary>
public sealed class StoryDetailResponseDto
{
    public Guid Id { get; init; }

    public string Title { get; init; }

    public string Slug { get; init; }

    public string Description { get; init; }

    public string CoverImageUrl { get; init; }

    public string Status { get; init; }

    public string ContentType { get; init; }

    public string OriginalSource { get; init; }

    public string Language { get; init; }

    public string AgeRating { get; init; }

    public long AuthorProfileId { get; init; }

    /// <summary>Pen name for a guest-published story (<see cref="AuthorProfileId"/> 0); otherwise null.</summary>
    public string GuestAuthorName { get; init; }

    public int ViewCount { get; init; }

    public int FollowCount { get; init; }

    public decimal RatingAvg { get; init; }

    public int RatingCount { get; init; }

    public DateTime? PublishedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public IReadOnlyList<StoryGenreDto> Genres { get; init; } = Array.Empty<StoryGenreDto>();

    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>Result of a one-call quick publish: the new story plus its first published chapter.</summary>
public sealed class QuickPublishStoryResultDto
{
    public StoryDetailResponseDto Story { get; init; }

    public ChapterDetailResponseDto FirstChapter { get; init; }
}

/// <summary>Story counts per lifecycle status for the admin dashboard (GET /v1/stories/admin/counts).</summary>
public sealed class StoryStatusCountsResponseDto
{
    /// <summary>Total number of stories across every status.</summary>
    public int Total { get; init; }

    /// <summary>Count keyed by <see cref="StoryStatus"/> name; every status is present, zero when none.</summary>
    public IReadOnlyDictionary<string, int> ByStatus { get; init; } =
        new Dictionary<string, int>();
}

public sealed class StoryGenreDto
{
    public string Name { get; init; }

    public string Slug { get; init; }

    public bool IsPrimary { get; init; }
}
