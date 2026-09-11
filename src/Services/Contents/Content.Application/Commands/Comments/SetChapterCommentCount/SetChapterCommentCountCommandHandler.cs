namespace Content.Application.Commands.Comments.SetChapterCommentCount;

public sealed class SetChapterCommentCountCommandHandler
    : ICommandHandler<SetChapterCommentCountCommand, ChapterCommentCountResponseDto>
{
    private readonly IChapterRepository _chapterRepository;
    private readonly ILogger<SetChapterCommentCountCommandHandler> _logger;

    public SetChapterCommentCountCommandHandler(
        IChapterRepository chapterRepository,
        ILogger<SetChapterCommentCountCommandHandler> logger)
    {
        _chapterRepository = chapterRepository;
        _logger = logger;
    }

    public async Task<ChapterCommentCountResponseDto> Handle(
        SetChapterCommentCountCommand request,
        CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken);
        if (chapter is null)
        {
            // No throw by contract: the caller (Community's best-effort sync) must
            // get a plain 404, not the global exception-handling pipeline.
            return null;
        }

        var count = Math.Max(0, request.Count);

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
