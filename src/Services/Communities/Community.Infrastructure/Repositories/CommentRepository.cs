namespace Community.Infrastructure.Repositories;

/// <summary>EF Core-backed repository for <see cref="Comment"/> reads and writes.</summary>
public sealed class CommentRepository : ICommentRepository
{
    private readonly CommunityDbContext _dbContext;

    /// <summary>Creates the repository over the scoped <see cref="CommunityDbContext"/>.</summary>
    public CommentRepository(CommunityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Finds a single comment by its public id, tracked (for subsequent updates).</summary>
    public Task<Comment> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    /// <summary>Batch-loads comments by public id, untracked. Unknown ids are silently omitted.</summary>
    public async Task<IReadOnlyList<Comment>> GetByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default)
    {
        // De-duplicate and drop empty ids; nothing to look up means an early return.
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

    /// <summary>Paged, newest-first list of visible comments for a chapter, plus the total matching count.</summary>
    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> GetVisibleByChapterAsync(
        Guid chapterId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.ChapterId == chapterId && x.Status == CommentStatus.Visible);

        // Count first for pagination metadata, then fetch the requested page.
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Counts visible comments for a chapter (used to keep Chapter.CommentCount in sync).</summary>
    public Task<int> CountVisibleByChapterAsync(Guid chapterId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Comments
            .AsNoTracking()
            .CountAsync(x => x.ChapterId == chapterId && x.Status == CommentStatus.Visible, cancellationToken);
    }

    /// <summary>Newest-first list of the most recent visible comments across all chapters.</summary>
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

    /// <summary>
    /// Admin moderation search: applies optional chapter/author/status filters and an
    /// ILIKE keyword filter on content, then returns a sorted, paged result with its total count.
    /// </summary>
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

        // Apply each optional filter only when a value was supplied.
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

        // Case-insensitive substring match on comment content.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Content, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sort direction is caller-controlled; default tie-break is by id.
        var ordered = sortAscending
            ? query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            : query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id);

        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Stages a new comment for insertion; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Comments.AddAsync(comment, cancellationToken);
    }

    /// <summary>Marks an already-tracked comment as modified.</summary>
    public void Update(Comment comment)
    {
        _dbContext.Comments.Update(comment);
    }

    /// <summary>Persists all pending changes to the database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
