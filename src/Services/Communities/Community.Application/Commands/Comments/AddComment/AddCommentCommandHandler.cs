namespace Community.Application.Commands.Comments.AddComment;

public sealed class AddCommentCommandHandler : ICommandHandler<AddCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<AddCommentCommandHandler> _logger;

    public AddCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        ILogger<AddCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
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

        // Integration point: publish a "comment added" event so the Content service can
        // increment Chapter.CommentCount and Notification can alert the chapter's author.
        // No event bus implementation exists yet (Phase 1).

        return CommunityDtoMapper.ToDto(comment);
    }
}
