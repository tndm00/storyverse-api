namespace Community.Application.Commands.Comments.EditComment;

public sealed class EditCommentCommandHandler : ICommandHandler<EditCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<EditCommentCommandHandler> _logger;

    public EditCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        ILogger<EditCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<CommentResponseDto> Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var comment = await _commentRepository.GetByPublicIdAsync(request.CommentId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.CommentNotFound);

        if (comment.AuthorUserId != userId)
        {
            throw new ForbiddenException(ApplicationErrorConstants.NotCommentAuthor);
        }

        if (comment.Status != CommentStatus.Visible)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.CommentNotEditable);
        }

        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;
        _commentRepository.Update(comment);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.CommentEdited, comment.PublicId, userId);

        return CommunityDtoMapper.ToDto(comment);
    }
}
