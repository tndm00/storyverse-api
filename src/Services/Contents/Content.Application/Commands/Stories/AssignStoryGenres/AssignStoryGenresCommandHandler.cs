namespace Content.Application.Commands.Stories.AssignStoryGenres;

public sealed class AssignStoryGenresCommandHandler : ICommandHandler<AssignStoryGenresCommand, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<AssignStoryGenresCommandHandler> _logger;

    public AssignStoryGenresCommandHandler(
        IStoryRepository storyRepository,
        IGenreRepository genreRepository,
        ICurrentAuthorContext authorContext,
        ILogger<AssignStoryGenresCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _genreRepository = genreRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<StoryDetailResponseDto> Handle(AssignStoryGenresCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var story = await _storyRepository.GetWithClassificationByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        if (story.AuthorProfileId != authorProfileId)
        {
            _logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        var requestedSlugs = request.Genres
            .Select(g => g.GenreSlug.Trim().ToLowerInvariant())
            .ToArray();

        var activeGenres = await _genreRepository.GetActiveBySlugsAsync(requestedSlugs, cancellationToken);
        if (activeGenres.Count != requestedSlugs.Length)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.GenreInactiveOrMissing);
        }

        var genreBySlug = activeGenres.ToDictionary(g => g.Slug, StringComparer.OrdinalIgnoreCase);

        story.Genres.Clear();
        foreach (var selection in request.Genres)
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

        story.UpdatedAt = DateTime.UtcNow;
        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryGenresAssigned, story.Id, story.Genres.Count);

        return ContentDtoMapper.ToDetail(story);
    }
}
