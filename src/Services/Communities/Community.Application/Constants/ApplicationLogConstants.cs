namespace Community.Application.Constants;

/// <summary>
/// Structured-logging message templates for the Community service. No secrets or
/// full comment/review bodies in these templates, per code-standard.md section 12.
/// </summary>
public static class ApplicationLogConstants
{
    public const string CommentAdded = "Comment {CommentId} added on chapter {ChapterId} by user {UserId}.";
    public const string CommentReplied = "Reply {CommentId} added to comment {ParentCommentId} by user {UserId}.";
    public const string CommentEdited = "Comment {CommentId} edited by user {UserId}.";
    public const string CommentDeleted = "Comment {CommentId} soft-deleted by user {UserId}.";
    public const string CommentHidden = "Comment {CommentId} hidden by moderator {UserId}.";
    public const string CommentUnhidden = "Comment {CommentId} unhidden by moderator {UserId}.";

    public const string RatingUpserted = "Rating {RatingId} for story {StoryId} set to {Score} by user {UserId}.";

    public const string VoteCast = "Vote recorded for story {StoryId} in week {WeekKey} by user {UserId}.";
    public const string VoteAlreadyCast = "User {UserId} already voted for story {StoryId} in week {WeekKey}; no-op.";
}
