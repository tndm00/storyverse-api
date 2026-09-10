namespace Community.Api.Constants;

/// <summary>
/// Centralized route segments, per code-standard.md section 11 (Constants Rules)
/// and api-guidelines.md sections 3-4 (Versioning, URI Design).
/// </summary>
public static class ControllerRouteConstants
{
    public const string ApiVersion1 = "v1";

    public const string CommentsBase = "v1/comments";
    public const string RatingsBase = "v1/ratings";
    public const string VotesBase = "v1/votes";

    public const string CommentByIdSegment = "{commentId:guid}";
    public const string CommentRepliesSegment = "{commentId:guid}/replies";
    public const string CommentHideSegment = "{commentId:guid}/hide";
    public const string CommentUnhideSegment = "{commentId:guid}/unhide";

    /// <summary>Internal (X-Service-Token): Moderation applies a Hide/restore decision to a comment.</summary>
    public const string CommentModerationVisibilitySegment = "{commentId:guid}/moderation-visibility";

    /// <summary>Internal (X-Service-Token): batch comment id -&gt; excerpt for the reports queue.</summary>
    public const string CommentExcerptsSegment = "internal/excerpts";

    public const string RatingsMineSegment = "mine";

    public const string VotesCountSegment = "count";
}
