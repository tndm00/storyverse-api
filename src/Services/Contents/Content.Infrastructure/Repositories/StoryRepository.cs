namespace Content.Infrastructure.Repositories;

public sealed class StoryRepository : IStoryRepository
{
    private readonly ContentDbContext _dbContext;

    public StoryRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Story> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Story> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public Task<Story> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<ContentTitleEntryDto>> GetTitlesByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default)
    {
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

    public Task<Story> GetWithClassificationByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories
            .Include(x => x.Genres).ThenInclude(sg => sg.Genre)
            .Include(x => x.Tags).ThenInclude(st => st.Tag)
            .FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.AnyAsync(x => x.Slug == slug, cancellationToken);
    }

    public Task<bool> AuthorHasStoryWithTitleAsync(long authorProfileId, string title, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories.AnyAsync(
            x => x.AuthorProfileId == authorProfileId && x.Title == title,
            cancellationToken);
    }

    public async Task<bool> HasExactlyOnePrimaryGenreAsync(long storyId, CancellationToken cancellationToken = default)
    {
        var primaryCount = await _dbContext.StoryGenres
            .CountAsync(x => x.StoryId == storyId && x.IsPrimary, cancellationToken);

        return primaryCount == 1;
    }

    public async Task<(IReadOnlyList<Story> Items, int TotalCount)> SearchPublishedAsync(
        StorySearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(
            _dbContext.Stories.AsNoTracking().Where(x => x.Status != StoryStatus.Draft),
            criteria);

        return await PageAsync(query, criteria, cancellationToken);
    }

    public async Task<(IReadOnlyList<Story> Items, int TotalCount)> SearchAllAsync(
        StorySearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(_dbContext.Stories.AsNoTracking(), criteria);

        return await PageAsync(query, criteria, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<StoryStatus, int>> CountByStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var grouped = await _dbContext.Stories
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var result = Enum.GetValues<StoryStatus>().ToDictionary(s => s, _ => 0);
        foreach (var row in grouped)
        {
            result[row.Status] = row.Count;
        }

        return result;
    }

    private static IQueryable<Story> ApplyFilters(IQueryable<Story> query, StorySearchCriteria criteria)
    {
        if (criteria.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
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

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var pattern = $"%{criteria.Keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, pattern));
        }

        return query;
    }

    private static async Task<(IReadOnlyList<Story> Items, int TotalCount)> PageAsync(
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

    public async Task AddAsync(Story story, CancellationToken cancellationToken = default)
    {
        await _dbContext.Stories.AddAsync(story, cancellationToken);
    }

    public void Update(Story story)
    {
        _dbContext.Stories.Update(story);
    }

    public Task IncrementViewCountAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Stories
            .Where(x => x.Id == storyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Story> ApplySort(IQueryable<Story> query, StorySearchCriteria criteria)
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
            _ => criteria.Descending
                ? query.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id)
                : query.OrderBy(x => x.PublishedAt).ThenBy(x => x.Id)
        };
    }
}
