namespace Library.Application.Queries.ReadingProgresses.GetStoryProgress;

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

    public async Task<ReadingProgressResponseDto> Handle(
        GetStoryProgressQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var progress = await _readingProgressRepository.GetAsync(userId, request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReadingProgressNotFound);

        return LibraryDtoMapper.ToDto(progress);
    }
}
