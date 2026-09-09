using Content.Domain.Entities;
using Content.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Content.Api.Seed;

/// <summary>
/// Dev-time genre bootstrap. Genres are an admin-managed closed list
/// (<c>POST /v1/genres</c> needs <c>genres.manage</c>), so a fresh database has
/// none — and publishing a story requires exactly one primary genre. This inserts
/// the standard eight setting-based genres on every Development startup so authors
/// can publish without an admin step first.
/// <para>
/// Idempotent: each genre is inserted only when its slug is absent; existing rows
/// are never modified or deleted. Runs in Development only.
/// </para>
/// </summary>
public sealed class GenreSeeder : IHostedService
{
    // Genres seeded in Development. The first two are the "Đăng truyện" top-level
    // categories (negative DisplayOrder → sorted first). The rest are reader-site
    // setting genres whose slugs double as the topic keys in story-fe siteContent.ts.
    private static readonly (string Name, string Slug, int DisplayOrder, string Description)[] Genres =
    {
        ("Sáng tác", "sang-tac", -2, "Truyện hư cấu do người viết sáng tác."),
        ("Chuyện có thật", "chuyen-co-that", -1, "Chuyện được kể lại là có thật."),
        ("Nhà hoang", "nha-hoang", 0, "Truyện lấy bối cảnh nhà hoang."),
        ("Miền núi", "mien-nui", 1, "Truyện lấy bối cảnh miền núi."),
        ("Bệnh viện", "benh-vien", 2, "Truyện lấy bối cảnh bệnh viện."),
        ("Sông nước", "song-nuoc", 3, "Truyện lấy bối cảnh sông nước."),
        ("Học đường", "hoc-duong", 4, "Truyện lấy bối cảnh học đường."),
        ("Làng quê", "lang-que", 5, "Truyện lấy bối cảnh làng quê."),
        ("Thành thị", "thanh-thi", 6, "Truyện lấy bối cảnh thành thị."),
        ("Thời chiến", "thoi-chien", 7, "Truyện lấy bối cảnh thời chiến."),
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GenreSeeder> _logger;

    public GenreSeeder(
        IServiceScopeFactory scopeFactory,
        IHostEnvironment environment,
        ILogger<GenreSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _environment = environment;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        try
        {
            var existing = await db.Genres
                .Select(g => g.Slug)
                .ToListAsync(cancellationToken);
            var known = existing.ToHashSet();

            var added = 0;
            foreach (var (name, slug, displayOrder, description) in Genres)
            {
                if (known.Contains(slug))
                {
                    continue;
                }

                db.Genres.Add(new Genre
                {
                    Name = name,
                    Slug = slug,
                    Description = description,
                    DisplayOrder = displayOrder,
                    IsActive = true,
                });
                added++;
            }

            if (added > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Genre seed: inserted {Count} genres.", added);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Genre seed failed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
