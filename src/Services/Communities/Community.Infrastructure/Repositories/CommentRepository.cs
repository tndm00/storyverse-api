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

    public async Task<IReadOnlyList<Comment>> GetByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default)
    {
        var ids = publicIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return Array.Empty<Comment>();
        }

        return await _dbContext.Comments
            .AsNoTracking()
            .Where(x => ids.Contains(x.PublicId))
            .ToListAsync(cancellationToken);
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

    public Task<int> CountVisibleByChapterAsync(Guid chapterId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments
            .AsNoTracking()
            .CountAsync(x => x.ChapterId == chapterId && x.Status == CommentStatus.Visible, cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetRecentVisibleAsync(
        int limit, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.Status == CommentStatus.Visible)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> SearchAsync(
        Guid? chapterId,
        long? authorUserId,
        CommentStatus? status,
        string keyword,
        bool sortAscending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Comments.AsNoTracking();

        if (chapterId is { } chapter && chapter != Guid.Empty)
        {
            query = query.Where(x => x.ChapterId == chapter);
        }

        if (authorUserId is { } author)
        {
            query = query.Where(x => x.AuthorUserId == author);
        }

        if (status is { } commentStatus)
        {
            query = query.Where(x => x.Status == commentStatus);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Content, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var ordered = sortAscending
            ? query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            : query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id);

        var items = await ordered
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
