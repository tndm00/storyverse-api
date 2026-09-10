namespace Content.Application.Commands.Moderation.SetChapterModerationVisibility;

public sealed class SetChapterModerationVisibilityCommandHandler
    : ICommandHandler<SetChapterModerationVisibilityCommand, ModerationVisibilityResponseDto>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger<SetChapterModerationVisibilityCommandHandler> _logger;

    public SetChapterModerationVisibilityCommandHandler(
        IChapterRepository chapterRepository,
        ILogger<SetChapterModerationVisibilityCommandHandler> logger)
    {
        _chapterRepository = chapterRepository;
        _logger = logger;
    }

    public async Task<ModerationVisibilityResponseDto> Handle(
        SetChapterModerationVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var now = DateTime.UtcNow;

        if (request.Hidden)
        {
            if (chapter.Status != ChapterStatus.Removed)
            {
                chapter.Status = ChapterStatus.Removed;
                chapter.RejectionReason = string.IsNullOrWhiteSpace(request.Reason) ? chapter.RejectionReason : request.Reason.Trim();
                chapter.UpdatedAt = now;
                _chapterRepository.Update(chapter);
                await _chapterRepository.SaveChangesAsync(cancellationToken);
            }
        }
        else if (chapter.Status == ChapterStatus.Removed)
        {
            // Restore a moderator-removed chapter to Published (it had passed review to be reportable).
            chapter.Status = ChapterStatus.Published;
            chapter.PublishedAt ??= now;
            chapter.UpdatedAt = now;
            _chapterRepository.Update(chapter);
            await _chapterRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            ApplicationLogConstants.ChapterModerationVisibilityChanged, chapter.Id, request.Hidden, chapter.Status);

        return new ModerationVisibilityResponseDto { Id = chapter.PublicId, Status = chapter.Status.ToString() };
    }
}
