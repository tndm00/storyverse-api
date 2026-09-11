namespace Community.Application.Commands.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IContentCommentCountSyncClient _commentCountSyncClient;
    private readonly ILogger<DeleteCommentCommandHandler> _logger;

    public DeleteCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        IContentCommentCountSyncClient commentCountSyncClient,
        ILogger<DeleteCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _commentCountSyncClient = commentCountSyncClient;
        _logger = logger;
    }

    public async Task<CommentResponseDto> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var comment = await _commentRepository.GetByPublicIdAsync(request.CommentId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.CommentNotFound);

        if (comment.AuthorUserId != userId)
        {
            throw new ForbiddenException(ApplicationErrorConstants.NotCommentAuthor);
        }

        if (comment.Status != CommentStatus.Deleted)
        {
            comment.Status = CommentStatus.Deleted;
            comment.UpdatedAt = DateTime.UtcNow;
            _commentRepository.Update(comment);
            await _commentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(ApplicationLogConstants.CommentDeleted, comment.PublicId, userId);

            // Best-effort sync of the real visible-comment count to Content; see
            // IContentCommentCountSyncClient.
            var commentCount = await _commentRepository.CountVisibleByChapterAsync(comment.ChapterId, cancellationToken);
            await _commentCountSyncClient.SyncCommentCountAsync(comment.ChapterId, commentCount, cancellationToken);
        }

        return CommunityDtoMapper.ToDto(comment);
    }
}
