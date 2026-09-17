namespace Community.Application.Commands.Comments.SetCommentVisibility;

/// <summary>Handles <see cref="SetCommentVisibilityCommand"/>: applies a moderator hide/restore decision to a comment.</summary>
public sealed class SetCommentVisibilityCommandHandler : ICommandHandler<SetCommentVisibilityCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IContentCommentCountSyncClient _commentCountSyncClient;
    private readonly ILogger<SetCommentVisibilityCommandHandler> _logger;

    /// <summary>Creates the handler with its repository, user context, sync client and logger dependencies.</summary>
    public SetCommentVisibilityCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        IContentCommentCountSyncClient commentCountSyncClient,
        ILogger<SetCommentVisibilityCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _commentCountSyncClient = commentCountSyncClient;
        _logger = logger;
    }

    /// <summary>Hides or restores a comment, skipping already soft-deleted comments and no-op state changes.</summary>
    public async Task<CommentResponseDto> Handle(SetCommentVisibilityCommand request, CancellationToken cancellationToken)
    {
        // A trusted service-to-service call (Moderation applying a report decision)
        // carries no user principal; fall back to 0 for the audit log line.
        var moderatorUserId = _userContext.IsAuthenticated ? _userContext.GetUserId() : 0L;

        // Load the comment or fail fast if it doesn't exist.
        var comment = await _commentRepository.GetByPublicIdAsync(request.CommentId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.CommentNotFound);

        // A comment the author soft-deleted is not resurfaced by a moderator restore.
        if (comment.Status != CommentStatus.Deleted)
        {
            var target = request.Hide ? CommentStatus.Hidden : CommentStatus.Visible;

            if (comment.Status != target)
            {
                comment.Status = target;
                comment.UpdatedAt = DateTime.UtcNow;
                _commentRepository.Update(comment);
                await _commentRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    request.Hide ? ApplicationLogConstants.CommentHidden : ApplicationLogConstants.CommentUnhidden,
                    comment.PublicId,
                    moderatorUserId);

                // Best-effort sync of the real visible-comment count to Content;
                // see IContentCommentCountSyncClient.
                var commentCount = await _commentRepository.CountVisibleByChapterAsync(comment.ChapterId, cancellationToken);
                await _commentCountSyncClient.SyncCommentCountAsync(comment.ChapterId, commentCount, cancellationToken);
            }
        }

        return CommunityDtoMapper.ToDto(comment);
    }
}
