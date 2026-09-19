namespace Content.Application.Commands.Stories.FlushViewCounts;

public sealed class FlushViewCountsCommandHandler
    : ICommandHandler<FlushViewCountsCommand, FlushViewCountsResultDto>
{
    private readonly IViewCountBuffer _viewCountBuffer;
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IContentUnitOfWork _unitOfWork;
    private readonly ILogger<FlushViewCountsCommandHandler> _logger;

    public FlushViewCountsCommandHandler(
        IViewCountBuffer viewCountBuffer,
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IContentUnitOfWork unitOfWork,
        ILogger<FlushViewCountsCommandHandler> logger)
    {
        _viewCountBuffer = viewCountBuffer;
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Takes the buffered counts, adds them to the story and chapter counters in one database
    /// transaction, and only then tells the buffer it may forget them. If anything fails before the
    /// commit, the batch stays parked in the buffer and is served again on the next tick.
    /// </summary>
    public async Task<FlushViewCountsResultDto> Handle(FlushViewCountsCommand request, CancellationToken cancellationToken)
    {
        // Atomically take whatever has been buffered since the last flush.
        var pending = await _viewCountBuffer.TakePendingAsync(cancellationToken);
        if (pending.IsEmpty)
        {
            return new FlushViewCountsResultDto();
        }

        // Apply story and chapter deltas together so they succeed or fail as one unit.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _storyRepository.AddViewCountsAsync(pending.Stories, ct);
            await _chapterRepository.AddViewCountsAsync(pending.Chapters, ct);
        }, cancellationToken);

        // Postgres has the batch; the buffer can safely drop its parked copy.
        await _viewCountBuffer.CommitAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.ViewCountFlushCompleted, pending.Stories.Count, pending.Chapters.Count);

        return new FlushViewCountsResultDto
        {
            StoryCount = pending.Stories.Count,
            ChapterCount = pending.Chapters.Count
        };
    }
}
