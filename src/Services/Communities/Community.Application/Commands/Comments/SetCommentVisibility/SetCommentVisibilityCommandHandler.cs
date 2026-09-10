namespace Community.Application.Commands.Comments.SetCommentVisibility;

public sealed class SetCommentVisibilityCommandHandler : ICommandHandler<SetCommentVisibilityCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<SetCommentVisibilityCommandHandler> _logger;

    public SetCommentVisibilityCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        ILogger<SetCommentVisibilityCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<CommentResponseDto> Handle(SetCommentVisibilityCommand request, CancellationToken cancellationToken)
    {
        // A trusted service-to-service call (Moderation applying a report decision)
        // carries no user principal; fall back to 0 for the audit log line.
        var moderatorUserId = _userContext.IsAuthenticated ? _userContext.GetUserId() : 0L;

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
            }
        }

        return CommunityDtoMapper.ToDto(comment);
    }
}
