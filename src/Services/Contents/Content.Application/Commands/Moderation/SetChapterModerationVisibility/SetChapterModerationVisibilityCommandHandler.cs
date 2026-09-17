namespace Content.Application.Commands.Moderation.SetChapterModerationVisibility;

public sealed class SetChapterModerationVisibilityCommandHandler
    : ICommandHandler<SetChapterModerationVisibilityCommand, ModerationVisibilityResponseDto>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger<SetChapterModerationVisibilityCommandHandler> _logger;

    /// <summary>Initializes the handler with the chapter repository and logger it depends on.</summary>
    public SetChapterModerationVisibilityCommandHandler(
        IChapterRepository chapterRepository,
        ILogger<SetChapterModerationVisibilityCommandHandler> logger)
    {
        _chapterRepository = chapterRepository;
        _logger = logger;
    }

    /// <summary>
    /// Applies a moderation Hide/Remove or restore decision to a chapter. Hide moves the
    /// chapter to Removed; restore brings a previously-removed chapter back to Published.
    /// </summary>
    public async Task<ModerationVisibilityResponseDto> Handle(
        SetChapterModerationVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        // Look up the chapter by its public id.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var now = DateTime.UtcNow;

        // Hide -> Removed; restoring a chapter that was never Removed is a no-op.
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
