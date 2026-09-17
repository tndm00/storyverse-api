namespace Content.Application.Commands.Chapters.RejectChapter;

public sealed class RejectChapterCommandHandler : ICommandHandler<RejectChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly IChapterReviewActionRepository _reviewActionRepository;
    private readonly IContentUnitOfWork _unitOfWork;
    private readonly ICurrentAuthorContext _currentUser;
    private readonly INotificationServiceClient _notificationClient;
    private readonly IAuthorDirectoryClient _authorDirectory;
    private readonly ILogger<RejectChapterCommandHandler> _logger;

    public RejectChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        IChapterReviewActionRepository reviewActionRepository,
        IContentUnitOfWork unitOfWork,
        ICurrentAuthorContext currentUser,
        INotificationServiceClient notificationClient,
        IAuthorDirectoryClient authorDirectory,
        ILogger<RejectChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _reviewActionRepository = reviewActionRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationClient = notificationClient;
        _authorDirectory = authorDirectory;
        _logger = logger;
    }

    /// <summary>
    /// Rejects a chapter in review, recording the reason and notifying the author.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(RejectChapterCommand request, CancellationToken cancellationToken)
    {
        var moderatorUserId = _currentUser.GetUserId();

        // Look up the chapter and ensure it is currently eligible for rejection.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        if (!ChapterStatusPolicy.CanReject(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        var now = DateTime.UtcNow;
        var reason = request.Reason.Trim();

        chapter.Status = ChapterStatus.Rejected;
        chapter.RejectionReason = reason;
        chapter.UpdatedAt = now;
        _chapterRepository.Update(chapter);

        var action = new ChapterReviewAction
        {
            ChapterId = chapter.Id,
            ModeratorUserId = moderatorUserId,
            Action = ChapterReviewActionType.Rejected,
            Note = reason,
            CreatedAt = now
        };

        // The chapter status change and its audit row must commit together.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _reviewActionRepository.AddAsync(action, ct);
            await _chapterRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterRejected, chapter.Id, moderatorUserId);

        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken);

        await NotifyAuthorAsync(chapter, story, reason, cancellationToken);

        var volumePublicId = await ResolveVolumePublicIdAsync(chapter.VolumeId, cancellationToken);
        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }

    /// <summary>
    /// Best-effort: tells the author their chapter was rejected and why. Runs
    /// after the reject transaction has committed; a failure here is logged,
    /// never thrown.
    /// </summary>
    private async Task NotifyAuthorAsync(Chapter chapter, Story story, string reason, CancellationToken cancellationToken)
    {
        if (!ChapterAuthorRecipient.TryResolveAuthorProfileId(story, out var authorProfileId))
        {
            _logger.LogInformation(
                ApplicationLogConstants.ChapterReviewNotificationSkippedGuest,
                chapter.Id, NotificationKind.ChapterRejected);
            return;
        }

        try
        {
            var authorUserId = await _authorDirectory.GetAuthorUserIdAsync(authorProfileId, cancellationToken);
            if (authorUserId is null)
            {
                _logger.LogWarning(
                    ApplicationLogConstants.ChapterReviewNotificationRecipientUnresolved,
                    chapter.Id, authorProfileId, NotificationKind.ChapterRejected);
                return;
            }

            await _notificationClient.SendAsync(
                authorUserId.Value,
                NotificationKind.ChapterRejected,
                "Chương của bạn bị từ chối",
                $"Chương \"{chapter.Title}\" của truyện \"{story.Title}\" bị từ chối. Lý do: {reason}",
                "Chapter",
                chapter.PublicId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                ApplicationLogConstants.ChapterReviewNotificationFailed,
                NotificationKind.ChapterRejected,
                chapter.Id,
                authorProfileId);
        }
    }

    /// <summary>
    /// Resolves a volume's public id from its internal id, if the chapter belongs to one.
    /// </summary>
    private async Task<Guid?> ResolveVolumePublicIdAsync(long? volumeId, CancellationToken cancellationToken)
    {
        if (volumeId is not { } id)
        {
            return null;
        }

        var volume = await _volumeRepository.GetByIdAsync(id, cancellationToken);
        return volume?.PublicId;
    }
}
