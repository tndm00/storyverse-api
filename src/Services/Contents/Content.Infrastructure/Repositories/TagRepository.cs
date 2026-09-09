namespace Content.Infrastructure.Repositories;

public sealed class TagRepository : ITagRepository
{
    private readonly ContentDbContext _dbContext;

    public TagRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Tag>> GetOrCreateBySlugAsync(
        IReadOnlyCollection<(string Name, string Slug)> tags,
        CancellationToken cancellationToken = default)
    {
        if (tags.Count == 0)
        {
            return Array.Empty<Tag>();
        }

        var slugs = tags.Select(t => t.Slug).Distinct().ToArray();

        var bySlug = (await _dbContext.Tags
                .Where(t => slugs.Contains(t.Slug))
                .ToListAsync(cancellationToken))
            .ToDictionary(t => t.Slug, StringComparer.Ordinal);

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

    public async Task<IReadOnlyList<Tag>> GetPopularAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tags
            .AsNoTracking()
            .OrderByDescending(x => x.UsageCount)
            .ThenBy(x => x.Name)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
