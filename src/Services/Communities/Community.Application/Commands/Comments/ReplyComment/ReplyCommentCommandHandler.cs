namespace Community.Application.Commands.Comments.ReplyComment;

public sealed class ReplyCommentCommandHandler : ICommandHandler<ReplyCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<ReplyCommentCommandHandler> _logger;

    public ReplyCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        ILogger<ReplyCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<CommentResponseDto> Handle(ReplyCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var parent = await _commentRepository.GetByPublicIdAsync(request.ParentCommentId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.CommentNotFound);

        if (parent.Status != CommentStatus.Visible)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ParentCommentNotVisible);
        }

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

        // Integration point: publish a "comment replied" event so Notification can
        // alert the parent comment's author. No event bus implementation yet (Phase 1).

        return CommunityDtoMapper.ToDto(reply);
    }
}
