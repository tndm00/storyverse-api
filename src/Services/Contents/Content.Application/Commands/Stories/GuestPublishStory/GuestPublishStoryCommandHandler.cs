namespace Content.Application.Commands.Stories.GuestPublishStory;

public sealed class GuestPublishStoryCommandHandler
    : ICommandHandler<GuestPublishStoryCommand, StoryDetailResponseDto>
{
    // Shared owner id for every anonymously published story. The `> 0` guards in
    // StoryRepository / GetStoriesQueryHandler already treat 0 as "no author".
    private const long GuestAuthorProfileId = 0;

    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IContentUnitOfWork _unitOfWork;
    private readonly ILogger<GuestPublishStoryCommandHandler> _logger;

    public GuestPublishStoryCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IGenreRepository genreRepository,
        IContentUnitOfWork unitOfWork,
        ILogger<GuestPublishStoryCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Creates a story and its first chapter for an anonymous guest, in a single transaction: the story
    /// is saved as a draft with resolved genres, then the chapter is added and submitted for moderation.
    /// </summary>
    public async Task<StoryDetailResponseDto> Handle(
        GuestPublishStoryCommand request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        Story story = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Story is owned by the shared guest account (id 0) with the pen name recorded instead of a real author.
            story = new Story
            {
                AuthorProfileId = GuestAuthorProfileId,
                GuestAuthorName = request.GuestPenName.Trim(),
                Title = title,
                Slug = await BuildUniqueSlugAsync(title, ct),
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? DescriptionExcerpt.From(request.ChapterContent)
                    : request.Description.Trim(),
                Status = StoryStatus.Draft,
                ContentType = StoryContentType.Original,
                Language = ApplicationConstants.DefaultLanguage,
                AgeRating = AgeRating.General
            };

            await _storyRepository.AddAsync(story, ct);

            // Validate and attach the requested genres to the new story.
            ApplyGenres(story, await ResolveGenresAsync(request.Genres, ct), request.Genres);

            // Save #1: persist the story + genre rows so the chapter FK resolves.
            await _storyRepository.SaveChangesAsync(ct);

            // Place the first chapter after any existing ones (none, in practice, for a brand-new story).
            var orderIndex = (await _chapterRepository.GetMaxOrderIndexAsync(story.Id, ct) ?? 0m)
                + ApplicationConstants.ChapterOrderIndexGap;

            var chapter = new Chapter
            {
                StoryId = story.Id,
                VolumeId = null,
                Title = title,
                OrderIndex = orderIndex,
                Content = request.ChapterContent,
                WordCount = WordCounter.Count(request.ChapterContent),
                // Submitted for moderation, not published directly — a moderator must
                // approve before the story goes live. See ApproveChapterCommandHandler.
                Status = ChapterStatus.PendingReview,
                AccessType = ChapterAccessType.Free
            };

            await _chapterRepository.AddAsync(chapter, ct);

            // Save #2: persist the chapter.
            await _chapterRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation("Guest quick-publish: story {StoryId} by '{PenName}'.", story.Id, story.GuestAuthorName);

        return ContentDtoMapper.ToDetail(story);
    }

    /// <summary>Generates a URL-safe slug from the title and appends a numeric suffix until it no longer collides with an existing story.</summary>
    private async Task<string> BuildUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        // Fall back to a random slug if the title yields nothing usable.
        var baseSlug = SlugGenerator.Generate(title);
        if (string.IsNullOrEmpty(baseSlug))
        {
            baseSlug = Guid.NewGuid().ToString("n")[..8];
        }

        // Keep incrementing the suffix until a non-colliding slug is found.
        var candidate = baseSlug;
        var suffix = 2;
        while (await _storyRepository.SlugExistsAsync(candidate, cancellationToken))
        {
            candidate = $"{baseSlug}{ApplicationConstants.SlugCollisionSeparator}{suffix}";
            suffix++;
        }

        return candidate;
    }

    /// <summary>Looks up the requested genres by slug and ensures every one of them exists and is active.</summary>
    private async Task<IReadOnlyDictionary<string, Genre>> ResolveGenresAsync(
        IReadOnlyList<StoryGenreSelection> selections,
        CancellationToken cancellationToken)
    {
        var slugs = selections
            .Select(g => g.GenreSlug.Trim().ToLowerInvariant())
            .ToArray();

        // All requested genres must exist and be active.
        var activeGenres = await _genreRepository.GetActiveBySlugsAsync(slugs, cancellationToken);
        if (activeGenres.Count != slugs.Length)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.GenreInactiveOrMissing);
        }

        return activeGenres.ToDictionary(g => g.Slug, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Attaches the resolved genres to the story, preserving each selection's primary flag.</summary>
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
}
