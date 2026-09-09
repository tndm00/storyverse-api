namespace Community.Infrastructure.Repositories;

public sealed class CommentRepository : ICommentRepository
{
    private readonly CommunityDbContext _dbContext;

    public CommentRepository(CommunityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Comment> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetVisibleByChapterAsync(
        Guid chapterId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.ChapterId == chapterId && x.Status == CommentStatus.Visible);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Comments.AddAsync(comment, cancellationToken);
    }

    public void Update(Comment comment)
    {
        _dbContext.Comments.Update(comment);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
