namespace Content.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IStoryRepository"/>, covering story
/// CRUD, classification lookups, public search/paging, and dashboard aggregates.
/// </summary>
public sealed class StoryRepository : IStoryRepository
{
    private readonly ContentDbContext _dbContext;

    public StoryRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Looks up a story by its internal numeric id.</summary>
    public Task<Story> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>Looks up a story by its public id.</summary>
    public Task<Story> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    /// <summary>Looks up a story by its URL slug.</summary>
    public Task<Story> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    /// <summary>
    /// Fetches lightweight title entries for a batch of story public ids, e.g. for
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

        return await _dbContext.Stories
            .AsNoTracking()
            .Where(x => ids.Contains(x.PublicId))
            .Select(x => new ContentTitleEntryDto { Id = x.PublicId, Title = x.Title })
            .ToListAsync(cancellationToken);
    }

    /// <summary>Loads a story together with its genre and tag classifications, tracked for editing.</summary>
    public Task<Story> GetWithClassificationByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories
            .Include(x => x.Genres).ThenInclude(sg => sg.Genre)
            .Include(x => x.Tags).ThenInclude(st => st.Tag)
            .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    /// <summary>Checks whether a story with the given slug already exists, for uniqueness validation.</summary>
    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.AnyAsync(x => x.Slug == slug, cancellationToken);
    }

    /// <summary>Checks whether an author already has a story with the given title, for duplicate-title validation.</summary>
    public Task<bool> AuthorHasStoryWithTitleAsync(long authorProfileId, string title, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.AnyAsync(
            x => x.AuthorProfileId == authorProfileId && x.Title == title,
            cancellationToken);
    }

    /// <summary>Verifies a story has exactly one genre flagged as primary, for classification validation.</summary>
    public async Task<bool> HasExactlyOnePrimaryGenreAsync(long storyId, CancellationToken cancellationToken = default)
    {
        var primaryCount = await _dbContext.StoryGenres
            .CountAsync(x => x.StoryId == storyId && x.IsPrimary, cancellationToken);

        return primaryCount == 1;
    }

    /// <summary>Searches non-draft (publicly visible) stories matching the given criteria, paged.</summary>
    public async Task<(IReadOnlyList<Story> Items, int TotalCount)> SearchPublishedAsync(
        StorySearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(
            _dbContext.Stories.AsNoTracking().Where(x => x.Status != StoryStatus.Draft),
            criteria);

        return await PageAsync(query, criteria, cancellationToken);
    }

    /// <summary>Searches all stories (including drafts) matching the given criteria, paged; used by admin/author views.</summary>
    public async Task<(IReadOnlyList<Story> Items, int TotalCount)> SearchAllAsync(
        StorySearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Stories.AsNoTracking(), criteria);

        return await PageAsync(query, criteria, cancellationToken);
    }

    /// <summary>Computes a dashboard count of stories per status, including statuses with zero stories.</summary>
    public async Task<IReadOnlyDictionary<StoryStatus, int>> CountByStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var grouped = await _dbContext.Stories
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Seed every enum value with 0 so callers don't need null-checks for unused statuses.
        var result = Enum.GetValues<StoryStatus>().ToDictionary(s => s, _ => 0);
        foreach (var row in grouped)
        {
            result[row.Status] = row.Count;
        }

        return result;
    }

    /// <summary>Loads stories by id, including genre classification, preserving the order of <paramref name="storyIds"/>.</summary>
    public async Task<IReadOnlyList<Story>> GetByIdsInOrderAsync(
        IReadOnlyList<long> storyIds, CancellationToken cancellationToken = default)
    {
        if (storyIds.Count == 0)
        {
            return Array.Empty<Story>();
        }

        var stories = await _dbContext.Stories
            .AsNoTracking()
            .Include(x => x.Genres).ThenInclude(sg => sg.Genre)
            .Where(x => storyIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // Re-apply the requested order, silently dropping any id that no longer exists.
        return storyIds
            .Where(stories.ContainsKey)
            .Select(id => stories[id])
            .ToArray();
    }

    /// <summary>
    /// Keyset-pages through all public (non-draft) stories ordered by id, including
    /// genre/tag classification, for bulk export or search re-indexing.
    /// </summary>
    public async Task<IReadOnlyList<Story>> GetAllPublicPagedAsync(
        long afterId, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stories
            .AsNoTracking()
            .Where(x => x.Status != StoryStatus.Draft && x.Id > afterId)
            .OrderBy(x => x.Id)
            .Include(x => x.Genres).ThenInclude(sg => sg.Genre)
            .Include(x => x.Tags).ThenInclude(st => st.Tag)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Finds ids of stories changed (created/updated) or whose chapters changed within a
    /// time window, used to drive incremental search-index sync.
    /// </summary>
    public async Task<IReadOnlyList<long>> GetStoryIdsChangedBetweenAsync(
        DateTime sinceExclusive, DateTime untilInclusive, int maxResults, CancellationToken cancellationToken = default)
    {
        // Stories updated directly within the window.
        var fromStories = await _dbContext.Stories
            .AsNoTracking()
            .Where(x => (x.UpdatedAt ?? x.CreatedAt) > sinceExclusive && (x.UpdatedAt ?? x.CreatedAt) <= untilInclusive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        // Stories whose chapters changed within the window (story content is a rollup of its chapters).
        var fromChapters = await _dbContext.Chapters
            .AsNoTracking()
            .Where(x => (x.UpdatedAt ?? x.CreatedAt) > sinceExclusive && (x.UpdatedAt ?? x.CreatedAt) <= untilInclusive)
            .Select(x => x.StoryId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return fromStories
            .Union(fromChapters)
            .Take(maxResults)
            .ToArray();
    }

    /// <summary>Applies status/length/author/genre/tag/keyword filters from <paramref name="criteria"/> to a stories query.</summary>
    private IQueryable<Story> ApplyFilters(IQueryable<Story> query, StorySearchCriteria criteria)
    {
        if (criteria.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        // "Long" means 2+ published chapters; "short" (default) means exactly one.
        if (criteria.ChapterLength is { } chapterLength)
        {
            query = chapterLength == ChapterLengthFilter.Long
                ? query.Where(x => _dbContext.Chapters.Count(c => c.StoryId == x.Id && c.Status == ChapterStatus.Published) >= 2)
                : query.Where(x => _dbContext.Chapters.Count(c => c.StoryId == x.Id && c.Status == ChapterStatus.Published) == 1);
        }

        if (criteria.AuthorProfileId is { } authorProfileId)
        {
            query = query.Where(x => x.AuthorProfileId == authorProfileId);
        }

        if (!string.IsNullOrEmpty(criteria.GenreSlug))
        {
            query = query.Where(x => x.Genres.Any(sg => sg.Genre.Slug == criteria.GenreSlug));
        }

        if (!string.IsNullOrEmpty(criteria.TagSlug))
        {
            query = query.Where(x => x.Tags.Any(st => st.Tag.Slug == criteria.TagSlug));
        }

        // Case-insensitive substring match on title.
        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var pattern = $"%{criteria.Keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, pattern));
        }

        return query;
    }

    /// <summary>Applies sorting and pagination to a filtered stories query, returning the page and total count.</summary>
    private async Task<(IReadOnlyList<Story> Items, int TotalCount)> PageAsync(
        IQueryable<Story> query,
        StorySearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await ApplySort(query, criteria)
            .Include(x => x.Genres).ThenInclude(sg => sg.Genre)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Queues a new story for insertion; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public async Task AddAsync(Story story, CancellationToken cancellationToken = default)
    {
        await _dbContext.Stories.AddAsync(story, cancellationToken);
    }

    /// <summary>Marks a tracked story as modified; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public void Update(Story story)
    {
        _dbContext.Stories.Update(story);
    }

    /// <summary>Queues a tracked story for deletion; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public void Remove(Story story)
    {
        _dbContext.Stories.Remove(story);
    }

    /// <summary>
    /// Atomically flips a story from Draft to Ongoing via a conditional UPDATE, when its
    /// first chapter is published, and stamps <c>PublishedAt</c> if not already set.
    /// </summary>
    public Task TryStartOngoingOnFirstChapterAsync(
        long storyId, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories
            .Where(x => x.Id == storyId && x.Status == StoryStatus.Draft)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Status, StoryStatus.Ongoing)
                    .SetProperty(x => x.PublishedAt, x => x.PublishedAt ?? nowUtc)
                    .SetProperty(x => x.UpdatedAt, x => nowUtc),
                cancellationToken);
    }

    /// <summary>Atomically increments a story's aggregate view counter in a single UPDATE.</summary>
    public Task IncrementViewCountAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories
            .Where(x => x.Id == storyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);
    }

    /// <summary>
    /// Adds each story's buffered delta onto its view counter with one atomic UPDATE per story
    /// (<c>ViewCount = ViewCount + delta</c>), so concurrent direct increments are never overwritten.
    /// </summary>
    public async Task AddViewCountsAsync(IReadOnlyDictionary<long, long> deltas, CancellationToken cancellationToken = default)
    {
        foreach (var (storyId, delta) in deltas)
        {
            var increment = (int)delta;

            await _dbContext.Stories
                .Where(x => x.Id == storyId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + increment), cancellationToken);
        }
    }

    /// <summary>Sums the view counter over every story (no rows loaded; the sum runs in the database).</summary>
    public async Task<long> SumViewCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Stories
            .AsNoTracking()
            .SumAsync(x => (long)x.ViewCount, cancellationToken);
    }

    /// <summary>Persists all pending changes tracked by the context.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Aggregates each story's total comment count (summed across its chapters) for the given story ids.</summary>
    public async Task<IReadOnlyDictionary<long, int>> GetCommentCountsAsync(
        IEnumerable<long> storyIds, CancellationToken cancellationToken = default)
    {
        var ids = storyIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<long, int>();
        }

        return await _dbContext.Chapters
            .AsNoTracking()
            .Where(c => ids.Contains(c.StoryId))
            .GroupBy(c => c.StoryId)
            .Select(g => new { StoryId = g.Key, Count = g.Sum(c => c.CommentCount) })
            .ToDictionaryAsync(x => x.StoryId, x => x.Count, cancellationToken);
    }

    /// <summary>Counts each story's published chapters for the given story ids.</summary>
    public async Task<IReadOnlyDictionary<long, int>> GetPublishedChapterCountsAsync(
        IEnumerable<long> storyIds, CancellationToken cancellationToken = default)
    {
        var ids = storyIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<long, int>();
        }

        return await _dbContext.Chapters
            .AsNoTracking()
            .Where(c => ids.Contains(c.StoryId) && c.Status == ChapterStatus.Published)
            .GroupBy(c => c.StoryId)
            .Select(g => new { StoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StoryId, x => x.Count, cancellationToken);
    }

    /// <summary>Applies the requested sort field/direction from <paramref name="criteria"/>, always tie-broken by Id.</summary>
    private IQueryable<Story> ApplySort(IQueryable<Story> query, StorySearchCriteria criteria)
    {
        // Always append Id as a tie-breaker so pagination stays stable.
        return criteria.SortBy switch
        {
            StorySortField.Title => criteria.Descending
                ? query.OrderByDescending(x => x.Title).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.Title).ThenBy(x => x.Id),
            StorySortField.ViewCount => criteria.Descending
                ? query.OrderByDescending(x => x.ViewCount).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.ViewCount).ThenBy(x => x.Id),
            StorySortField.RatingAvg => criteria.Descending
                ? query.OrderByDescending(x => x.RatingAvg).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.RatingAvg).ThenBy(x => x.Id),
            StorySortField.CreatedAt => criteria.Descending
                ? query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            // Computed on the fly (SUM over the story's chapters) rather than a
            // denormalized Story.CommentCount column, to avoid a second sync tier
            // on top of the Chapter.CommentCount sync already coming from Community.
            StorySortField.CommentCount => criteria.Descending
                ? query.OrderByDescending(x => _dbContext.Chapters.Where(c => c.StoryId == x.Id).Sum(c => (int?)c.CommentCount) ?? 0).ThenByDescending(x => x.Id)
                : query.OrderBy(x => _dbContext.Chapters.Where(c => c.StoryId == x.Id).Sum(c => (int?)c.CommentCount) ?? 0).ThenBy(x => x.Id),
            _ => criteria.Descending
                ? query.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.PublishedAt).ThenBy(x => x.Id)
        };
    }
}
