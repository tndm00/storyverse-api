namespace Community.Application.Commands.Comments.AddComment;

public sealed class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IContentCommentCountSyncClient _commentCountSyncClient;
    private readonly ILogger<AddCommentCommandHandler> _logger;

    public AddCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        IContentCommentCountSyncClient commentCountSyncClient,
        ILogger<AddCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _commentCountSyncClient = commentCountSyncClient;
        _logger = logger;
    }

    public async Task<CommentResponseDto> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var comment = new Comment
        {
            ChapterId = request.ChapterId,
            AuthorUserId = userId,
            ParentCommentId = null,
            Content = request.Content.Trim(),
            Status = CommentStatus.Visible
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.CommentAdded, comment.PublicId, comment.ChapterId, userId);

        // Sync the real visible-comment count to Content outside the write above
        // (already committed) so Chapter.CommentCount reflects every comment, not
        // an in-memory increment. Best-effort by contract (see
        // IContentCommentCountSyncClient): a sync failure is logged there and
        // never propagates here.
        var commentCount = await _commentRepository.CountVisibleByChapterAsync(comment.ChapterId, cancellationToken);
        await _commentCountSyncClient.SyncCommentCountAsync(comment.ChapterId, commentCount, cancellationToken);

        // Integration point: publish a "comment added" event so Notification can
        // alert the chapter's author. No event bus implementation exists yet (Phase 1).

        return CommunityDtoMapper.ToDto(comment);
    }
}
