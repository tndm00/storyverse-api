namespace Library.Application.Commands.ReadingProgresses.UpsertReadingProgress;

public sealed class UpsertReadingProgressCommandHandler
    : ICommandHandler<UpsertReadingProgressCommand, ReadingProgressResponseDto>
{
    private readonly IReadingProgressRepository _readingProgressRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<UpsertReadingProgressCommandHandler> _logger;

    public UpsertReadingProgressCommandHandler(
        IReadingProgressRepository readingProgressRepository,
        ICurrentUserContext currentUser,
        ILogger<UpsertReadingProgressCommandHandler> logger)
    {
        _readingProgressRepository = readingProgressRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReadingProgressResponseDto> Handle(
        UpsertReadingProgressCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();
        var now = DateTime.UtcNow;

        var progress = await _readingProgressRepository.GetAsync(userId, request.StoryId, cancellationToken);

        if (progress is null)
        {
            progress = new ReadingProgress
            {
                UserId = userId,
                StoryId = request.StoryId,
                LastChapterId = request.LastChapterId,
                ScrollPercent = request.ScrollPercent,
                LastReadAt = now
            };

            await _readingProgressRepository.AddAsync(progress, cancellationToken);
        }
        else
        {
            progress.LastChapterId = request.LastChapterId;
            progress.ScrollPercent = request.ScrollPercent;
            progress.LastReadAt = now;
            progress.UpdatedAt = now;
            _readingProgressRepository.Update(progress);
        }

        await _readingProgressRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.ReadingProgressUpserted, userId, request.StoryId, request.LastChapterId);

        return LibraryDtoMapper.ToDto(progress);
    }
}
