namespace Content.Application.Commands.Stories.QuickPublishStory;

public sealed class QuickPublishStoryCommandHandler
    : ICommandHandler<QuickPublishStoryCommand, QuickPublishStoryResultDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IContentUnitOfWork _unitOfWork;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<QuickPublishStoryCommandHandler> _logger;

    public QuickPublishStoryCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IGenreRepository genreRepository,
        ITagRepository tagRepository,
        IContentUnitOfWork unitOfWork,
        ICurrentAuthorContext authorContext,
        ILogger<QuickPublishStoryCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _genreRepository = genreRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<QuickPublishStoryResultDto> Handle(
        QuickPublishStoryCommand request,
        CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();
        var title = request.Title.Trim();

        Story story = null;
        Chapter chapter = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Double-submit guard: an author cannot quick-publish two works with the same title.
            if (await _storyRepository.AuthorHasStoryWithTitleAsync(authorProfileId, title, ct))
            {
                throw new ConflictException(ApplicationErrorConstants.DuplicateQuickPublish);
            }

            story = new Story
            {
                AuthorProfileId = authorProfileId,
                Title = title,
                Slug = await BuildUniqueSlugAsync(title, ct),
                Description = request.Description,
                CoverImageUrl = request.CoverImageUrl,
                Status = StoryStatus.Draft,
                ContentType = request.ContentType,
                OriginalSource = request.ContentType == StoryContentType.Translated ? request.OriginalSource : null,
                Language = string.IsNullOrWhiteSpace(request.Language)
                    ? ApplicationConstants.DefaultLanguage
                    : request.Language.Trim(),
                AgeRating = request.AgeRating
            };

            await _storyRepository.AddAsync(story, ct);

            ApplyGenres(story, await ResolveGenresAsync(request.Genres, ct), request.Genres);
            ApplyTags(story, await ResolveTagsAsync(request.Tags, ct));

            // Save #1: persist the story + its genre/tag rows so the chapter FK resolves.
            await _storyRepository.SaveChangesAsync(ct);

            var orderIndex = (await _chapterRepository.GetMaxOrderIndexAsync(story.Id, ct) ?? 0m)
                + ApplicationConstants.ChapterOrderIndexGap;

            chapter = new Chapter
            {
                StoryId = story.Id,
                VolumeId = null,
                Title = string.IsNullOrWhiteSpace(request.ChapterTitle) ? title : request.ChapterTitle.Trim(),
                OrderIndex = orderIndex,
                Content = request.ChapterContent,
                WordCount = WordCounter.Count(request.ChapterContent),
                // Submitted for moderation, not published directly — a moderator must
                // approve before the story goes live. See ApproveChapterCommandHandler.
                // `CompleteImmediately` has no effect until then; the author can set
                // Completed manually via POST /v1/stories/{id}/status after approval.
                Status = ChapterStatus.PendingReview,
                AccessType = ChapterAccessType.Free
            };

            await _chapterRepository.AddAsync(chapter, ct);

            // Save #2: commit the pending-review chapter alongside the story/genre rows.
            await _chapterRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.StoryQuickPublished,
            story.Id,
            authorProfileId,
            chapter.Id,
            request.CompleteImmediately);

        return new QuickPublishStoryResultDto
        {
            Story = ContentDtoMapper.ToDetail(story),
            FirstChapter = ContentDtoMapper.ToDetail(chapter, story.PublicId, null)
        };
    }

    private async Task<string> BuildUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(title);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = Guid.NewGuid().ToString("n")[..8];
        }

        var candidate = baseSlug;
        var suffix = 2;
        while (await _storyRepository.SlugExistsAsync(candidate, cancellationToken))
        {
            candidate = $"{baseSlug}{ApplicationConstants.SlugCollisionSeparator}{suffix}";
            suffix++;
        }

        return candidate;
    }

    private async Task<IReadOnlyDictionary<string, Genre>> ResolveGenresAsync(
        IReadOnlyList<StoryGenreSelection> selections,
        CancellationToken cancellationToken)
    {
        var slugs = selections
            .Select(g => g.GenreSlug.Trim().ToLowerInvariant())
            .ToArray();

        var activeGenres = await _genreRepository.GetActiveBySlugsAsync(slugs, cancellationToken);
        if (activeGenres.Count != slugs.Length)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.GenreInactiveOrMissing);
        }

        return activeGenres.ToDictionary(g => g.Slug, StringComparer.OrdinalIgnoreCase);
    }

    private static void ApplyGenres(
        Story story,
        IReadOnlyDictionary<string, Genre> genreBySlug,
        IReadOnlyList<StoryGenreSelection> selections)
    {
        foreach (var selection in selections)
        {
            var genre = genreBySlug[selection.GenreSlug.Trim().ToLowerInvariant()];
            story.Genres.Add(new StoryGenre
            {
                StoryId = story.Id,
                GenreId = genre.Id,
                IsPrimary = selection.IsPrimary,
                Genre = genre
            });
        }
    }

    private async Task<IReadOnlyList<Tag>> ResolveTagsAsync(
        IReadOnlyList<string> tagNames,
        CancellationToken cancellationToken)
    {
        var normalized = tagNames
            .Select(name => (Name: (name ?? string.Empty).Trim(), Slug: SlugGenerator.Generate(name ?? string.Empty)))
            .Where(t => t.Slug.Length > 0)
            .GroupBy(t => t.Slug)
            .Select(g => g.First())
            .ToArray();

        return normalized.Length == 0
            ? Array.Empty<Tag>()
            : (await _tagRepository.GetOrCreateBySlugAsync(normalized, cancellationToken)).ToArray();
    }

    private static void ApplyTags(Story story, IReadOnlyList<Tag> tags)
    {
        foreach (var tag in tags)
        {
            tag.UsageCount += 1;
            story.Tags.Add(new StoryTag { StoryId = story.Id, TagId = tag.Id, Tag = tag });
        }
    }
}
