namespace Content.Infrastructure.Repositories;

public sealed class VolumeRepository : IVolumeRepository
{
    private readonly ContentDbContext _dbContext;

    public VolumeRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Volume> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Volumes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Volume> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Volumes.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<IReadOnlyList<Volume>> GetByStoryAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Volumes
            .AsNoTracking()
            .Where(x => x.StoryId == storyId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Volume>> GetByStoryTrackedAsync(
        long storyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Volumes
            .Where(x => x.StoryId == storyId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public Task<int?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Volumes
            .Where(x => x.StoryId == storyId)
            .MaxAsync(x => (int?)x.OrderIndex, cancellationToken);
    }

    public async Task AddAsync(Volume volume, CancellationToken cancellationToken = default)
    {
        await _dbContext.Volumes.AddAsync(volume, cancellationToken);
    }

    public void Update(Volume volume)
    {
        _dbContext.Volumes.Update(volume);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
