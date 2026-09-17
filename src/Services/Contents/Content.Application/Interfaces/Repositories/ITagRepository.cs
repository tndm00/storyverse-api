namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for the open-ended <see cref="Tag"/> list.
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// Resolves each requested name to an existing tag (matched by slug) or a new
    /// tracked tag. Callers are responsible for maintaining <see cref="Tag.UsageCount"/>.
    /// </summary>
    Task<IReadOnlyList<Tag>> GetOrCreateBySlugAsync(
        IReadOnlyCollection<(string Name, string Slug)> tags,
        CancellationToken cancellationToken = default);

    /// <summary>Most-used tags, ordered by <see cref="Tag.UsageCount"/> descending, capped at <paramref name="count"/>.</summary>
    Task<IReadOnlyList<Tag>> GetPopularAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Persists all pending changes tracked by this repository's unit of work.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
