namespace Community.Application.Commands.Comments.DeleteComment;

/// <summary>Handles <see cref="DeleteCommentCommand"/>: soft-deletes a comment owned by the caller.</summary>
public sealed class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IContentCommentCountSyncClient _commentCountSyncClient;
    private readonly ILogger<DeleteCommentCommandHandler> _logger;

    /// <summary>Creates the handler with its repository, user context, sync client and logger dependencies.</summary>
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

    /// <summary>Soft-deletes the caller's own comment and, if it changed state, resyncs the chapter's comment count.</summary>
    public async Task<CommentResponseDto> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        // Load the comment or fail fast if it doesn't exist.
        var comment = await _commentRepository.GetByPublicIdAsync(request.CommentId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.CommentNotFound);

        // Only the original author may delete their own comment.
        if (comment.AuthorUserId != userId)
        {
            throw new ForbiddenException(ApplicationErrorConstants.NotCommentAuthor);
        }

        // Only mutate and resync if it isn't already deleted (idempotent).
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
