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

    public async Task<StoryDetailResponseDto> Handle(
        GuestPublishStoryCommand request,
        CancellationToken cancellationToken)
    {
        var title = request.Title.Trim();
        Story story = null;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            story = new Story
            {
                AuthorProfileId = GuestAuthorProfileId,
                GuestAuthorName = request.GuestPenName.Trim(),
                Title = title,
                Slug = await BuildUniqueSlugAsync(title, ct),
                Description = request.Description,
                Status = StoryStatus.Draft,
                ContentType = StoryContentType.Original,
                Language = ApplicationConstants.DefaultLanguage,
                AgeRating = AgeRating.General
            };

            await _storyRepository.AddAsync(story, ct);

            ApplyGenres(story, await ResolveGenresAsync(request.Genres, ct), request.Genres);

            // Save #1: persist the story + genre rows so the chapter FK resolves.
            await _storyRepository.SaveChangesAsync(ct);

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

            await _chapterRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation("Guest quick-publish: story {StoryId} by '{PenName}'.", story.Id, story.GuestAuthorName);

        return ContentDtoMapper.ToDetail(story);
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
}
