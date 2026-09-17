namespace Content.Application.Commands.Comments.SetChapterCommentCount;

public sealed class SetChapterCommentCountCommandHandler
    : ICommandHandler<SetChapterCommentCountCommand, ChapterCommentCountResponseDto>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger<SetChapterCommentCountCommandHandler> _logger;

    /// <summary>Initializes the handler with the chapter repository and logger it depends on.</summary>
    public SetChapterCommentCountCommandHandler(
        IChapterRepository chapterRepository,
        ILogger<SetChapterCommentCountCommandHandler> logger)
    {
        _chapterRepository = chapterRepository;
        _logger = logger;
    }

    /// <summary>Overwrites a chapter's denormalized comment count with the value pushed by the Community service.</summary>
    public async Task<ChapterCommentCountResponseDto> Handle(
        SetChapterCommentCountCommand request,
        CancellationToken cancellationToken)
    {
        // Look up the chapter by its public id.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken);
        if (chapter is null)
        {
            // No throw by contract: the caller (Community's best-effort sync) must
            // get a plain 404, not the global exception-handling pipeline.
            return null;
        }

        // Guard against negative counts before applying them.
        var count = Math.Max(0, request.Count);

        // Persist the updated comment count.
        chapter.CommentCount = count;
        chapter.UpdatedAt = DateTime.UtcNow;
        _chapterRepository.Update(chapter);
        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterCommentCountUpdated, chapter.PublicId, count);

        return new ChapterCommentCountResponseDto
        {
            Id = chapter.PublicId,
            CommentCount = chapter.CommentCount
        };
    }
}
