namespace Content.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IChapterRepository"/>, providing chapter
/// CRUD, review-queue queries, scheduled-publish handling and view-count tracking.
/// </summary>
public sealed class ChapterRepository : IChapterRepository
{
    private readonly ContentDbContext _dbContext;

    public ChapterRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Looks up a single chapter by its public id.</summary>
    public Task<Chapter> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    /// <summary>
    /// Fetches lightweight title entries for a batch of chapter public ids, e.g. for
    /// cross-referencing in notifications or search results.
    /// </summary>
    public async Task<IReadOnlyList<ContentTitleEntryDto>> GetTitlesByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default)
    {
        // Filter out empty ids and dedupe before querying.
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

    /// <summary>
    /// Fetches chapter context (its own title plus the owning story's id/slug/title) for a
    /// batch of chapter public ids, joining Chapters with Stories.
    /// </summary>
    public async Task<IReadOnlyList<ChapterContextEntryDto>> GetContextByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default)
    {
        // Filter out empty ids and dedupe before querying.
        var ids = publicIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return Array.Empty<ChapterContextEntryDto>();
        }

        return await (
            from chapter in _dbContext.Chapters.AsNoTracking()
            join story in _dbContext.Stories.AsNoTracking() on chapter.StoryId equals story.Id
            where ids.Contains(chapter.PublicId)
            select new ChapterContextEntryDto
            {
                ChapterId = chapter.PublicId,
                ChapterTitle = chapter.Title,
                StoryId = story.PublicId,
                StorySlug = story.Slug,
                StoryTitle = story.Title
            }).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Lists a story's chapters in order, either all non-removed chapters or only the
    /// published ones depending on <paramref name="publishedOnly"/>.
    /// </summary>
    public async Task<IReadOnlyList<Chapter>> GetByStoryAsync(
        long storyId,
        bool publishedOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Chapters
            .AsNoTracking()
            .Where(x => x.StoryId == storyId);

        // Public callers only see published chapters; internal callers see everything but removed ones.
        query = publishedOnly
            ? query.Where(x => x.Status == ChapterStatus.Published)
            : query.Where(x => x.Status != ChapterStatus.Removed);

        return await query.OrderBy(x => x.OrderIndex).ToListAsync(cancellationToken);
    }

    /// <summary>Checks whether a story has at least one published chapter.</summary>
    public Task<bool> StoryHasPublishedChapterAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters.AnyAsync(
            x => x.StoryId == storyId && x.Status == ChapterStatus.Published,
            cancellationToken);
    }

    /// <summary>
    /// Concatenates the content of all published chapters of a story, in order, for use
    /// cases like full-text export or search indexing.
    /// </summary>
    public async Task<string> GetPublishedContentByStoryIdAsync(long storyId, CancellationToken cancellationToken = default)
    {
        var chapters = await _dbContext.Chapters
            .AsNoTracking()
            .Where(x => x.StoryId == storyId && x.Status == ChapterStatus.Published)
            .OrderBy(x => x.OrderIndex)
            .Select(x => x.Content)
            .ToListAsync(cancellationToken);

        return string.Join("\n\n", chapters);
    }

    /// <summary>
    /// Loads a volume's non-removed chapters in order, tracked by EF Core so callers can
    /// mutate and save them (e.g. reordering).
    /// </summary>
    public async Task<IReadOnlyList<Chapter>> GetByVolumeTrackedAsync(
        long volumeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Chapters
            .Where(x => x.VolumeId == volumeId && x.Status != ChapterStatus.Removed)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Loads a story's non-removed chapters that are not assigned to any volume, tracked
    /// by EF Core so callers can mutate and save them (e.g. reordering).
    /// </summary>
    public async Task<IReadOnlyList<Chapter>> GetStoryChaptersWithoutVolumeTrackedAsync(
        long storyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Chapters
            .Where(x => x.StoryId == storyId && x.VolumeId == null && x.Status != ChapterStatus.Removed)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Fetches scheduled chapters whose scheduled publish time has arrived, up to
    /// <paramref name="maxItems"/>, for a background publisher job to process.
    /// </summary>
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

    /// <summary>
    /// Atomically flips a chapter from Scheduled to Published via a conditional UPDATE, so
    /// only one concurrent caller can win the transition. Returns whether this call did it.
    /// </summary>
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

    /// <summary>
    /// Pages through chapters awaiting moderation (PendingReview/InReview, or a single
    /// given status), joined with their owning story, optionally filtered by a keyword
    /// matched against chapter or story title.
    /// </summary>
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

        // Optional keyword filter, case-insensitive against chapter/story title.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.chapter.Title, pattern) || EF.Functions.ILike(x.story.Title, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Page the results, oldest submissions first.
        var page = await query
            .OrderBy(x => x.chapter.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = page.Select(x => (x.chapter, x.story)).ToArray();
        return (items, totalCount);
    }

    /// <summary>
    /// Pages through chapters already reviewed (Published or Rejected, matching
    /// <paramref name="actionType"/>), joined with their owning story, optionally filtered
    /// by a keyword matched against chapter or story title.
    /// </summary>
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

        // Optional keyword filter, case-insensitive against chapter/story title.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x =>
                EF.Functions.ILike(x.chapter.Title, pattern) || EF.Functions.ILike(x.story.Title, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Page the results, most recently reviewed first.
        var page = await query
            .OrderByDescending(x => x.chapter.UpdatedAt)
            .ThenByDescending(x => x.chapter.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = page.Select(x => (x.chapter, x.story)).ToArray();
        return (items, totalCount);
    }

    /// <summary>
    /// Computes review-queue dashboard counts: chapters currently pending/in-review, plus
    /// the distinct chapter count ever approved or rejected (from the audit trail).
    /// </summary>
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

    /// <summary>
    /// Returns the highest chapter <c>OrderIndex</c> currently used in a story (excluding
    /// removed chapters), so a new chapter can be appended after it.
    /// </summary>
    public Task<decimal?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters
            .Where(x => x.StoryId == storyId && x.Status != ChapterStatus.Removed)
            .MaxAsync(x => (decimal?)x.OrderIndex, cancellationToken);
    }

    /// <summary>Queues a new chapter for insertion; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default)
    {
        await _dbContext.Chapters.AddAsync(chapter, cancellationToken);
    }

    /// <summary>Marks a tracked chapter as modified; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public void Update(Chapter chapter)
    {
        _dbContext.Chapters.Update(chapter);
    }

    /// <summary>
    /// Atomically increments both the chapter's and its owning story's view counters in a
    /// single UPDATE each, avoiding read-modify-write races under concurrent reads.
    /// </summary>
    public async Task IncrementViewCountAsync(long chapterId, long storyId, CancellationToken cancellationToken = default)
    {
        // Bump the chapter's own view count.
        await _dbContext.Chapters
            .Where(x => x.Id == chapterId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);

        // Bump the aggregate view count on the owning story.
        await _dbContext.Stories
            .Where(x => x.Id == storyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);
    }

    /// <summary>
    /// Adds each chapter's buffered delta onto its view counter with one atomic UPDATE per chapter
    /// (<c>ViewCount = ViewCount + delta</c>), so concurrent direct increments are never overwritten.
    /// </summary>
    public async Task AddViewCountsAsync(IReadOnlyDictionary<long, long> deltas, CancellationToken cancellationToken = default)
    {
        foreach (var (chapterId, delta) in deltas)
        {
            var increment = (int)delta;

            await _dbContext.Chapters
                .Where(x => x.Id == chapterId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + increment), cancellationToken);
        }
    }

    /// <summary>Persists all pending changes tracked by the context.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
