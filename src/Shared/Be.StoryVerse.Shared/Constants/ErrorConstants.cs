namespace Be.StoryVerse.Shared.Constants;

/// <summary>
/// Stable, reusable error codes shared across services for the
/// <c>ResponseDto&lt;T&gt;</c> error envelope.
/// </summary>
public static class ErrorConstants
{
    public const string BadRequest = "error_bad_request";
    public const string Unauthorized = "error_unauthorized";
    public const string Forbidden = "error_forbidden";
    public const string NotFound = "error_not_found";
    public const string Conflict = "error_conflict";
    public const string ValidationFailed = "error_validation_failed";
    public const string Unexpected = "error_unexpected";
}
