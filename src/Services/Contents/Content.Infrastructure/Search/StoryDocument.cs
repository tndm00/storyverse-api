namespace Content.Infrastructure.Search;

/// <summary>
/// Elasticsearch document shape for the <c>storyverse-stories</c> index. Not
/// a source of truth — Postgres (<see cref="Story"/>/<see cref="Chapter"/>)
/// always is; this only needs enough fields to search/filter/sort and return
/// story ids for <see cref="Content.Application.Interfaces.Repositories.IStoryRepository.GetByIdsInOrderAsync"/>
/// to hydrate.
/// </summary>
public sealed class StoryDocument
{
    /// <summary>Postgres <see cref="Story"/> id — also used as the ES document id.</summary>
    public long StoryId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Concatenated Published-chapter text, capped so index size/indexing cost
    /// stays bounded — full per-chapter search granularity is a stated non-goal
    /// for v1 (see the Elasticsearch/Kibana plan, section 2).
    /// </summary>
    public string ChapterContent { get; init; } = string.Empty;

    public long AuthorProfileId { get; init; }

    public string GuestAuthorName { get; init; }

    public IReadOnlyList<string> GenreSlugs { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> GenreNames { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> TagSlugs { get; init; } = Array.Empty<string>();

    public string Status { get; init; } = string.Empty;

    public string Language { get; init; } = string.Empty;

    public DateTime? PublishedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public int ViewCount { get; init; }

    public decimal RatingAvg { get; init; }

    public int RatingCount { get; init; }
}
