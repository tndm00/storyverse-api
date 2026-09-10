namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="Volume"/>.
/// </summary>
public interface IVolumeRepository
{
    Task<Volume> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Volume> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Volume>> GetByStoryAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>A story's volumes as change-tracked entities, for a bulk reorder.</summary>
    Task<IReadOnlyList<Volume>> GetByStoryTrackedAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>Highest order index among a story's volumes, or null when it has none.</summary>
    Task<int?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default);

    Task AddAsync(Volume volume, CancellationToken cancellationToken = default);

    void Update(Volume volume);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
