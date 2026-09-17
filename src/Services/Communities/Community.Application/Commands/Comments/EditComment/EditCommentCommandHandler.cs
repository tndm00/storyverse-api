namespace Community.Application.Commands.Comments.EditComment;

/// <summary>Handles <see cref="EditCommentCommand"/>: updates the content of the caller's own visible comment.</summary>
public sealed class EditCommentCommandHandler : ICommandHandler<EditCommentCommand, CommentResponseDto>
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<EditCommentCommandHandler> _logger;

    /// <summary>Creates the handler with its repository, user context and logger dependencies.</summary>
    public EditCommentCommandHandler(
        ICommentRepository commentRepository,
        ICurrentUserContext userContext,
        ILogger<EditCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _userContext = userContext;
        _logger = logger;
    }

    /// <summary>Updates the caller's own comment content, only when it exists, is owned by them, and is still visible.</summary>
    public async Task<CommentResponseDto> Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        // Load the comment or fail fast if it doesn't exist.
        var comment = await _commentRepository.GetByPublicIdAsync(request.CommentId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.CommentNotFound);

        // Only the original author may edit their own comment.
        if (comment.AuthorUserId != userId)
        {
            throw new ForbiddenException(ApplicationErrorConstants.NotCommentAuthor);
        }

        // Hidden/deleted comments can no longer be edited.
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
