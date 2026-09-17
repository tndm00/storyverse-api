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

    /// <summary>Replaces a story's genre assignments after verifying ownership and that all requested genres are active.</summary>
    public async Task<StoryDetailResponseDto> Handle(AssignStoryGenresCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Load the story with its classification data (genres/tags) included.
        var story = await _storyRepository.GetWithClassificationByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Only the owning author may reassign genres.
        if (story.AuthorProfileId != authorProfileId)
        {
            _logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        var requestedSlugs = request.Genres
            .Select(g => g.GenreSlug.Trim().ToLowerInvariant())
            .ToArray();

        // All requested genres must exist and be active.
        var activeGenres = await _genreRepository.GetActiveBySlugsAsync(requestedSlugs, cancellationToken);
        if (activeGenres.Count != requestedSlugs.Length)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.GenreInactiveOrMissing);
        }

        var genreBySlug = activeGenres.ToDictionary(g => g.Slug, StringComparer.OrdinalIgnoreCase);

        // Replace the story's entire genre set with the requested selections.
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

        // Persist the change.
        story.UpdatedAt = DateTime.UtcNow;
        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryGenresAssigned, story.Id, story.Genres.Count);

        return ContentDtoMapper.ToDetail(story);
    }
}
