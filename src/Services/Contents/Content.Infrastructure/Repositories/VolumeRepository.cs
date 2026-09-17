namespace Content.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IVolumeRepository"/>, covering volume
/// CRUD and ordered listing per story.
/// </summary>
public sealed class VolumeRepository : IVolumeRepository
{
    private readonly ContentDbContext _dbContext;

    public VolumeRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Looks up a volume by its internal numeric id.</summary>
    public Task<Volume> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Volumes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>Looks up a volume by its public id.</summary>
    public Task<Volume> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Volumes.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    /// <summary>Lists a story's volumes in order, untracked, for read-only display.</summary>
    public async Task<IReadOnlyList<Volume>> GetByStoryAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Volumes
            .AsNoTracking()
            .Where(x => x.StoryId == storyId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Lists a story's volumes in order, tracked by EF Core so callers can mutate and save them (e.g. reordering).</summary>
    public async Task<IReadOnlyList<Volume>> GetByStoryTrackedAsync(
        long storyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Volumes
            .Where(x => x.StoryId == storyId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Returns the highest volume <c>OrderIndex</c> currently used in a story, so a new volume can be appended after it.</summary>
    public Task<int?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Volumes
            .Where(x => x.StoryId == storyId)
            .MaxAsync(x => (int?)x.OrderIndex, cancellationToken);
    }

    /// <summary>Queues a new volume for insertion; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public async Task AddAsync(Volume volume, CancellationToken cancellationToken = default)
    {
        await _dbContext.Volumes.AddAsync(volume, cancellationToken);
    }

    /// <summary>Marks a tracked volume as modified; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public void Update(Volume volume)
    {
        _dbContext.Volumes.Update(volume);
    }

    /// <summary>Persists all pending changes tracked by the context.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
