namespace Community.Application.Commands.Comments.DeleteComment;

/// <summary>Soft-deletes the caller's own comment (transitions it to Deleted).</summary>
public sealed class DeleteCommentCommand : ICommand<CommentResponseDto>
{
    public Guid CommentId { get; init; }
}
