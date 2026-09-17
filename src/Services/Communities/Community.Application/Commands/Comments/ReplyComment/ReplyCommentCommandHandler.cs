namespace Community.Application.Commands.Comments.ReplyComment;

/// <summary>Handles <see cref="ReplyCommentCommand"/>: adds a reply under an existing top-level comment.</summary>
public sealed class ReplyCommentCommandHandler : ICommandHandler<ReplyCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IContentCommentCountSyncClient _commentCountSyncClient;
    private readonly ILogger<ReplyCommentCommandHandler> _logger;

    /// <summary>Creates the handler with its repository, user context, sync client and logger dependencies.</summary>
    public ReplyCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        IContentCommentCountSyncClient commentCountSyncClient,
        ILogger<ReplyCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _commentCountSyncClient = commentCountSyncClient;
        _logger = logger;
    }

    /// <summary>Adds a reply to a visible, top-level comment, then best-effort syncs the chapter's comment count.</summary>
    public async Task<CommentResponseDto> Handle(ReplyCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        // Load the parent comment or fail fast if it doesn't exist.
        var parent = await _commentRepository.GetByPublicIdAsync(request.ParentCommentId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.CommentNotFound);

        // Can't reply to a hidden/deleted comment.
        if (parent.Status != CommentStatus.Visible)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ParentCommentNotVisible);
        }

        // Only one level of nesting is allowed: replies to replies are rejected.
        if (parent.ParentCommentId is not null)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.CannotReplyToReply);
        }

        var reply = new Comment
        {
            ChapterId = parent.ChapterId,
            AuthorUserId = userId,
            ParentCommentId = parent.PublicId,
            Content = request.Content.Trim(),
            Status = CommentStatus.Visible
        };

        await _commentRepository.AddAsync(reply, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.CommentReplied, reply.PublicId, parent.PublicId, userId);

        // Best-effort sync of the real visible-comment count to Content; see
        // IContentCommentCountSyncClient.
        var commentCount = await _commentRepository.CountVisibleByChapterAsync(reply.ChapterId, cancellationToken);
        await _commentCountSyncClient.SyncCommentCountAsync(reply.ChapterId, commentCount, cancellationToken);

        // Integration point: publish a "comment replied" event so Notification can
        // alert the parent comment's author. No event bus implementation yet (Phase 1).

        return CommunityDtoMapper.ToDto(reply);
    }
}
