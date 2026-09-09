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

    Task<IReadOnlyList<Tag>> GetPopularAsync(int count, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
