namespace Library.Application.Queries.ReadingProgresses.GetStoryProgress;

/// <summary>
/// Handles <see cref="GetStoryProgressQuery"/> by returning the authenticated
/// caller's last recorded reading position in a story.
/// </summary>
public sealed class GetStoryProgressQueryHandler : IQueryHandler<GetStoryProgressQuery, ReadingProgressResponseDto>
{
    private readonly IReadingProgressRepository _readingProgressRepository;
    private readonly ICurrentUserContext _currentUser;

    public GetStoryProgressQueryHandler(
        IReadingProgressRepository readingProgressRepository,
        ICurrentUserContext currentUser)
    {
        _readingProgressRepository = readingProgressRepository;
        _currentUser = currentUser;
    }

    /// <summary>Returns the caller's reading progress for the given story, or throws when none exists.</summary>
    public async Task<ReadingProgressResponseDto> Handle(
        GetStoryProgressQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        // Progress must already exist for this user/story.
        var progress = await _readingProgressRepository.GetAsync(userId, request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReadingProgressNotFound);

        return LibraryDtoMapper.ToDto(progress);
    }
}
