namespace Community.Application.Constants;

/// <summary>
/// Centralized business error messages surfaced through the standard error
/// envelope, per code-standard.md section 11.
/// </summary>
public static class ApplicationErrorConstants
{
    public const string UserNotAuthenticated = "The request is not associated with an authenticated user.";

    public const string CommentNotFound = "Comment not found.";
    public const string RatingNotFound = "You have not rated this story yet.";

    public const string NotCommentAuthor = "You can only edit or delete your own comments.";
    public const string CommentNotEditable = "This comment can no longer be edited.";
    public const string ParentCommentNotVisible = "You cannot reply to a comment that is hidden or deleted.";
    public const string CannotReplyToReply = "Replies can only be made on a top-level comment.";

    public const string CommentContentRequired = "Comment content is required.";
    public const string ReviewTextTooLong = "The review text is too long.";
    public const string InvalidRatingScore = "The rating score must be between 1 and 5.";
    public const string InvalidPageParameters = "Invalid pagination parameters.";
}
