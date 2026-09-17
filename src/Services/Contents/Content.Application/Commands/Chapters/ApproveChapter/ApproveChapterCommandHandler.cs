namespace Content.Application.Commands.Chapters.ApproveChapter;

public sealed class ApproveChapterCommandHandler : ICommandHandler<ApproveChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly IChapterReviewActionRepository _reviewActionRepository;
    private readonly IContentUnitOfWork _unitOfWork;
    private readonly ICurrentAuthorContext _currentUser;
    private readonly INotificationServiceClient _notificationClient;
    private readonly IAuthorDirectoryClient _authorDirectory;
    private readonly ILogger<ApproveChapterCommandHandler> _logger;

    public ApproveChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        IChapterReviewActionRepository reviewActionRepository,
        IContentUnitOfWork unitOfWork,
        ICurrentAuthorContext currentUser,
        INotificationServiceClient notificationClient,
        IAuthorDirectoryClient authorDirectory,
        ILogger<ApproveChapterCommandHandler> logger)
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
    /// Approves a chapter in review, publishing it and, if this is the story's
    /// first approved chapter, flipping the story from Draft to Ongoing.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(ApproveChapterCommand request, CancellationToken cancellationToken)
    {
        var moderatorUserId = _currentUser.GetUserId();

        // Look up the chapter and ensure it is currently eligible for approval.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        if (!ChapterStatusPolicy.CanApprove(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken);
        var now = DateTime.UtcNow;

        // Publish the chapter.
        chapter.Status = ChapterStatus.Published;
        chapter.PublishedAt ??= now;
        chapter.RejectionReason = null;
        chapter.UpdatedAt = now;
        _chapterRepository.Update(chapter);

        // First approved chapter takes the story out of Draft.
        var storyChanged = false;
        if (story.Status == StoryStatus.Draft)
        {
            story.Status = StoryStatus.Ongoing;
            story.PublishedAt ??= now;
            story.UpdatedAt = now;
            _storyRepository.Update(story);
            storyChanged = true;
        }

        var action = new ChapterReviewAction
        {
            ChapterId = chapter.Id,
            ModeratorUserId = moderatorUserId,
            Action = ChapterReviewActionType.Approved,
            Note = null,
            CreatedAt = now
        };

        // The chapter status change and its audit row must commit together.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _reviewActionRepository.AddAsync(action, ct);
            await _chapterRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        if (storyChanged)
        {
            _logger.LogInformation(ApplicationLogConstants.StoryAutoOngoing, story.Id);
        }

        _logger.LogInformation(ApplicationLogConstants.ChapterApproved, chapter.Id, story.Id);

        await NotifyAuthorAsync(chapter, story, cancellationToken);

        var volumePublicId = await ResolveVolumePublicIdAsync(chapter.VolumeId, cancellationToken);
        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }

    /// <summary>
    /// Best-effort: tells the author their chapter is live. Runs after the
    /// approve transaction has committed; a failure here is logged, never thrown.
    /// </summary>
    private async Task NotifyAuthorAsync(Chapter chapter, Story story, CancellationToken cancellationToken)
    {
        if (!ChapterAuthorRecipient.TryResolveAuthorProfileId(story, out var authorProfileId))
        {
            _logger.LogInformation(
                ApplicationLogConstants.ChapterReviewNotificationSkippedGuest,
                chapter.Id, NotificationKind.ChapterApproved);
            return;
        }

        try
        {
            var authorUserId = await _authorDirectory.GetAuthorUserIdAsync(authorProfileId, cancellationToken);
            if (authorUserId is null)
            {
                _logger.LogWarning(
                    ApplicationLogConstants.ChapterReviewNotificationRecipientUnresolved,
                    chapter.Id, authorProfileId, NotificationKind.ChapterApproved);
                return;
            }

            await _notificationClient.SendAsync(
                authorUserId.Value,
                NotificationKind.ChapterApproved,
                "Chương của bạn đã được duyệt",
                $"Chương \"{chapter.Title}\" của truyện \"{story.Title}\" đã được duyệt và đăng.",
                "Chapter",
                chapter.PublicId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                ApplicationLogConstants.ChapterReviewNotificationFailed,
                NotificationKind.ChapterApproved,
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
