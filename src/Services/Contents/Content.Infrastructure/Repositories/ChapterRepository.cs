namespace Content.Infrastructure.Repositories;

public sealed class ChapterRepository : IChapterRepository
{
    private readonly ContentDbContext _dbContext;

    public ChapterRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Chapter> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<IReadOnlyList<ContentTitleEntryDto>> GetTitlesByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default)
    {
        var ids = publicIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return Array.Empty<ContentTitleEntryDto>();
        }

        return await _dbContext.Chapters
            .AsNoTracking()
            .Where(x => ids.Contains(x.PublicId))
            .Select(x => new ContentTitleEntryDto { Id = x.PublicId, Title = x.Title })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Chapter>> GetByStoryAsync(
        long storyId,
        bool publishedOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Chapters
            .AsNoTracking()
            .Where(x => x.StoryId == storyId);

        query = publishedOnly
            ? query.Where(x => x.Status == ChapterStatus.Published)
            : query.Where(x => x.Status != ChapterStatus.Removed);

        return await query.OrderBy(x => x.OrderIndex).ToListAsync(cancellationToken);
    }

    public Task<bool> StoryHasPublishedChapterAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters.AnyAsync(
            x => x.StoryId == storyId && x.Status == ChapterStatus.Published,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Chapter>> GetByVolumeTrackedAsync(
        long volumeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Chapters
            .Where(x => x.VolumeId == volumeId && x.Status != ChapterStatus.Removed)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Chapter>> GetStoryChaptersWithoutVolumeTrackedAsync(
        long storyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Chapters
            .Where(x => x.StoryId == storyId && x.VolumeId == null && x.Status != ChapterStatus.Removed)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Chapter>> GetDueScheduledAsync(
        DateTime asOfUtc, int maxItems, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Chapters
            .AsNoTracking()
            .Where(x => x.Status == ChapterStatus.Scheduled && x.ScheduledAt != null && x.ScheduledAt <= asOfUtc)
            .OrderBy(x => x.ScheduledAt)
            .Take(maxItems)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TryMarkPublishedAsync(
        long chapterId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        // Conditional UPDATE: only the caller that still sees status = Scheduled
        // performs the flip, so concurrent publishers cannot double-publish.
        var affected = await _dbContext.Chapters
            .Where(x => x.Id == chapterId && x.Status == ChapterStatus.Scheduled)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, ChapterStatus.Published)
                    .SetProperty(x => x.PublishedAt, x => x.PublishedAt ?? x.ScheduledAt ?? nowUtc)
                    .SetProperty(x => x.UpdatedAt, x => nowUtc),
                cancellationToken);

        return affected == 1;
    }

    public async Task<(IReadOnlyList<(Chapter Chapter, Story Story)> Items, int TotalCount)> GetPendingReviewAsync(
        ChapterStatus? status,
        string keyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var statuses = status is { } s
            ? new[] { s }
            : new[] { ChapterStatus.PendingReview, ChapterStatus.InReview };

        var query =
            from chapter in _dbContext.Chapters.AsNoTracking()
            join story in _dbContext.Stories.AsNoTracking() on chapter.StoryId equals story.Id
            where statuses.Contains(chapter.Status)
            select new { chapter, story };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.chapter.Title, pattern) || EF.Functions.ILike(x.story.Title, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderBy(x => x.chapter.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = page.Select(x => (x.chapter, x.story)).ToArray();
        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<(Chapter Chapter, Story Story)> Items, int TotalCount)> GetReviewedAsync(
        ChapterReviewActionType actionType,
        string keyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Filter by the chapter's CURRENT status, not "ever had this action in its
        // history" — a chapter that was Rejected and later re-reviewed + approved
        // must leave the Rejected list (and appear as Approved instead).
        var status = actionType == ChapterReviewActionType.Rejected
            ? ChapterStatus.Rejected
            : ChapterStatus.Published;

        var query =
            from chapter in _dbContext.Chapters.AsNoTracking()
            join story in _dbContext.Stories.AsNoTracking() on chapter.StoryId equals story.Id
            where chapter.Status == status
            select new { chapter, story };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.chapter.Title, pattern) || EF.Functions.ILike(x.story.Title, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderByDescending(x => x.chapter.UpdatedAt)
            .ThenByDescending(x => x.chapter.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = page.Select(x => (x.chapter, x.story)).ToArray();
        return (items, totalCount);
    }

    public async Task<(int Pending, int InReview, int Approved, int Rejected)> GetReviewCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var pending = await _dbContext.Chapters.AsNoTracking()
            .CountAsync(x => x.Status == ChapterStatus.PendingReview, cancellationToken);
        var inReview = await _dbContext.Chapters.AsNoTracking()
            .CountAsync(x => x.Status == ChapterStatus.InReview, cancellationToken);

        var approved = await _dbContext.ChapterReviewActions.AsNoTracking()
            .Where(a => a.Action == ChapterReviewActionType.Approved)
            .Select(a => a.ChapterId)
            .Distinct()
            .CountAsync(cancellationToken);
        var rejected = await _dbContext.ChapterReviewActions.AsNoTracking()
            .Where(a => a.Action == ChapterReviewActionType.Rejected)
            .Select(a => a.ChapterId)
            .Distinct()
            .CountAsync(cancellationToken);

        return (pending, inReview, approved, rejected);
    }

    public Task<decimal?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters
            .Where(x => x.StoryId == storyId && x.Status != ChapterStatus.Removed)
            .MaxAsync(x => (decimal?)x.OrderIndex, cancellationToken);
    }

    public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default)
    {
        await _dbContext.Chapters.AddAsync(chapter, cancellationToken);
    }

    public void Update(Chapter chapter)
    {
        _dbContext.Chapters.Update(chapter);
    }

    public async Task IncrementViewCountAsync(long chapterId, long storyId, CancellationToken cancellationToken = default)
    {
        await _dbContext.Chapters
            .Where(x => x.Id == chapterId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);

        await _dbContext.Stories
            .Where(x => x.Id == storyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
