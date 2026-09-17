namespace Content.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="ITagRepository"/>. Tags are free-form and
/// created on demand as authors tag their stories.
/// </summary>
public sealed class TagRepository : ITagRepository
{
    private readonly ContentDbContext _dbContext;

    public TagRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Resolves a batch of (name, slug) pairs to existing <see cref="Tag"/> rows, creating
    /// (and queuing for insertion) any tag whose slug doesn't already exist.
    /// </summary>
    public async Task<IReadOnlyList<Tag>> GetOrCreateBySlugAsync(
        IReadOnlyCollection<(string Name, string Slug)> tags,
        CancellationToken cancellationToken = default)
    {
        if (tags.Count == 0)
        {
            return Array.Empty<Tag>();
        }

        var slugs = tags.Select(t => t.Slug).Distinct().ToArray();

        // Load all already-existing tags matching the requested slugs in one query.
        var bySlug = (await _dbContext.Tags
                .Where(t => slugs.Contains(t.Slug))
                .ToListAsync(cancellationToken))
            .ToDictionary(t => t.Slug, StringComparer.Ordinal);

        // For each requested tag, reuse the existing row or create a new one.
        var result = new List<Tag>(tags.Count);
        foreach (var (name, slug) in tags)
        {
            if (bySlug.TryGetValue(slug, out var existing))
            {
                result.Add(existing);
                continue;
            }

            var created = new Tag { Name = name, Slug = slug, UsageCount = 0 };
            await _dbContext.Tags.AddAsync(created, cancellationToken);
            bySlug[slug] = created;
            result.Add(created);
        }

        return result;
    }

    /// <summary>Lists the most-used tags, for tag-cloud/suggestion UI.</summary>
    public async Task<IReadOnlyList<Tag>> GetPopularAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .AsNoTracking()
            .OrderByDescending(x => x.UsageCount)
            .ThenBy(x => x.Name)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Persists all pending changes tracked by the context.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
