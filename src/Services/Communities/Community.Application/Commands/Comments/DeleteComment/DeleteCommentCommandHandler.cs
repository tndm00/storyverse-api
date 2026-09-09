namespace Community.Application.Commands.Comments.DeleteComment;

public sealed class DeleteCommentCommandHandler : ICommandHandler<DeleteCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<DeleteCommentCommandHandler> _logger;

    public DeleteCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        ILogger<DeleteCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
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
        }

        return CommunityDtoMapper.ToDto(comment);
    }
}
