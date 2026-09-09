namespace Content.Application.Commands.Chapters.CreateChapter;

public sealed class CreateChapterCommandHandler : ICommandHandler<CreateChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<CreateChapterCommandHandler> _logger;

    public CreateChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<CreateChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<ChapterDetailResponseDto> Handle(CreateChapterCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        long? volumeId = null;
        if (request.VolumeId is { } volumePublicId)
        {
            var volume = await _volumeRepository.GetByPublicIdAsync(volumePublicId, cancellationToken)
                ?? throw new NotFoundException(ApplicationErrorConstants.VolumeNotFound);

            if (volume.StoryId != story.Id)
            {
                throw new BusinessRuleException(ApplicationErrorConstants.VolumeStoryMismatch);
            }

            volumeId = volume.Id;
        }

        var autoAssignedOrder = request.OrderIndex <= 0;
        var orderIndex = autoAssignedOrder
            ? (await _chapterRepository.GetMaxOrderIndexAsync(story.Id, cancellationToken) ?? 0m)
              + ApplicationConstants.ChapterOrderIndexGap
            : request.OrderIndex;

        var now = DateTime.UtcNow;

        var chapter = new Chapter
        {
            StoryId = story.Id,
            VolumeId = volumeId,
            Title = request.Title.Trim(),
            OrderIndex = orderIndex,
            Content = request.Content,
            WordCount = WordCounter.Count(request.Content),
            Status = ChapterStatus.Draft,
            AccessType = ChapterAccessType.Free
        };

        if (request.PublishImmediately)
        {
            if (!await _storyRepository.HasExactlyOnePrimaryGenreAsync(story.Id, cancellationToken))
            {
                throw new BusinessRuleException(ApplicationErrorConstants.PrimaryGenreRequired);
            }

            // Submitted for moderation, not published directly — a moderator must
            // approve before the chapter (and, on first approval, the story) goes
            // live. See ApproveChapterCommandHandler for the Draft -> Ongoing flip.
            chapter.Status = ChapterStatus.PendingReview;
        }

        await _chapterRepository.AddAsync(chapter, cancellationToken);
        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterCreated, chapter.Id, story.Id);

        if (autoAssignedOrder)
        {
            _logger.LogInformation(ApplicationLogConstants.ChapterOrderAutoAssigned, chapter.Id, orderIndex, story.Id);
        }

        if (request.PublishImmediately)
        {
            _logger.LogInformation(ApplicationLogConstants.ChapterSubmittedForReview, chapter.Id, story.Id);
        }

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, request.VolumeId);
    }
}
