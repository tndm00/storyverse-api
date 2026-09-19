namespace Content.Application.Dtos;

/// <summary>One story in the "most viewed today" ranking.</summary>
public sealed class TopStoryViewsResponseDto
{
    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    /// <summary>Views the story received today.</summary>
    public long Views { get; init; }
}

/// <summary>
/// Platform-wide view statistics for the admin (PlatformAdmin only). The daily figures come from
/// Redis and are null when Redis is unavailable, so the UI can show "not available" instead of a
/// misleading zero.
/// </summary>
public sealed class ViewStatsResponseDto
{
    /// <summary>Lifetime views across all stories (Postgres counters plus views still buffered in Redis).</summary>
    public long TotalViews { get; init; }

    /// <summary>Views recorded today (Vietnam business day), or null when unavailable.</summary>
    public long? TodayViews { get; init; }

    /// <summary>Views recorded yesterday (Vietnam business day), or null when unavailable.</summary>
    public long? YesterdayViews { get; init; }

    /// <summary>First day the daily counters were kept — earlier days were never recorded, so they read as zero.</summary>
    public DateOnly? TrackingSince { get; init; }

    /// <summary>The most viewed stories today, highest first.</summary>
    public IReadOnlyList<TopStoryViewsResponseDto> TopStoriesToday { get; init; } = Array.Empty<TopStoryViewsResponseDto>();
}
