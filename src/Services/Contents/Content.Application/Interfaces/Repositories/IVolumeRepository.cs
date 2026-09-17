namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="Volume"/>.
/// </summary>
public interface IVolumeRepository
{
    /// <summary>Loads a single volume by its internal id.</summary>
    Task<Volume> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Loads a single volume by its public-facing id.</summary>
    Task<Volume> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    /// <summary>Lists a story's volumes.</summary>
    Task<IReadOnlyList<Volume>> GetByStoryAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>A story's volumes as change-tracked entities, for a bulk reorder.</summary>
    Task<IReadOnlyList<Volume>> GetByStoryTrackedAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>Highest order index among a story's volumes, or null when it has none.</summary>
    Task<int?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>Registers a new volume to be inserted on the next <see cref="SaveChangesAsync"/>.</summary>
    Task AddAsync(Volume volume, CancellationToken cancellationToken = default);

    /// <summary>Marks a tracked volume as modified for the next <see cref="SaveChangesAsync"/>.</summary>
    void Update(Volume volume);

    /// <summary>Persists all pending changes tracked by this repository's unit of work.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
