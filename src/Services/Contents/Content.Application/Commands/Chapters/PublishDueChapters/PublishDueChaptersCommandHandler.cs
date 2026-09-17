namespace Content.Application.Commands.Chapters.PublishDueChapters;

/// <summary>
/// Turns due <see cref="ChapterStatus.Scheduled"/> chapters into
/// <see cref="ChapterStatus.Published"/> (product-workflow-context.md section 5.3:
/// "Scheduled chapters become Published automatically at their scheduled time").
/// <para>
/// Scheduled chapters are author-owned and never enter the moderator review
/// queue (<see cref="ChapterStatusPolicy.CanSchedule"/> only allows
/// Draft -&gt; Scheduled), so this publishes them directly, exactly as the
/// state diagram Draft -&gt; Scheduled -&gt; Published prescribes. No
/// <c>ChapterReviewAction</c> row is written — this is not a moderator action.
/// </para>
/// <para>
/// Concurrency: each chapter is claimed by a single conditional
/// <c>UPDATE ... WHERE status = 'Scheduled'</c>; the chapter flip and the
/// parent-story Draft-&gt;Ongoing flip run in one transaction per chapter. Safe
/// to run from multiple instances and safe to re-run.
/// </para>
/// </summary>
public sealed class PublishDueChaptersCommandHandler
    : ICommandHandler<PublishDueChaptersCommand, PublishDueChaptersResultDto>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly IStoryRepository _storyRepository;
    private readonly IContentUnitOfWork _unitOfWork;
    private readonly ILogger<PublishDueChaptersCommandHandler> _logger;

    public PublishDueChaptersCommandHandler(
        IChapterRepository chapterRepository,
        IStoryRepository storyRepository,
        IContentUnitOfWork unitOfWork,
        ILogger<PublishDueChaptersCommandHandler> logger)
    {
        _chapterRepository = chapterRepository;
        _storyRepository = storyRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Scans for due scheduled chapters, claims and publishes each one in its
    /// own transaction, and flips the parent story out of Draft on first publish.
    /// </summary>
    public async Task<PublishDueChaptersResultDto> Handle(
        PublishDueChaptersCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var due = await _chapterRepository.GetDueScheduledAsync(now, request.BatchSize, cancellationToken);

        if (due.Count == 0)
        {
            return new PublishDueChaptersResultDto { PublishedCount = 0, DueCount = 0 };
        }

        var published = 0;

        // Claim and publish each due chapter independently so one failure/race
        // doesn't block the rest of the batch.
        foreach (var chapter in due)
        {
            var didPublish = false;

            await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
            {
                if (!await _chapterRepository.TryMarkPublishedAsync(chapter.Id, now, innerCt))
                {
                    // Lost the race to another instance, or the chapter was
                    // rescheduled/cancelled between the scan and this update.
                    return;
                }

                didPublish = true;
                await _storyRepository.TryStartOngoingOnFirstChapterAsync(chapter.StoryId, now, innerCt);
            }, cancellationToken);

            if (didPublish)
            {
                published++;
                _logger.LogInformation(
                    ApplicationLogConstants.ScheduledChapterAutoPublished,
                    chapter.Id, chapter.StoryId, chapter.ScheduledAt);
            }
        }

        _logger.LogInformation(
            ApplicationLogConstants.ScheduledChapterPublisherCompleted, published, due.Count);

        return new PublishDueChaptersResultDto { PublishedCount = published, DueCount = due.Count };
    }
}
