namespace Moderation.Application.Constants;

/// <summary>
/// Centralized business error messages surfaced through the standard error
/// envelope, per code-standard.md section 11.
/// </summary>
public static class ApplicationErrorConstants
{
    public const string CallerNotAuthenticated = "The request is not authenticated.";

    public const string ReportNotFound = "Report not found.";

    public const string ReportNotPending = "Only a pending report can be picked up for review.";
    public const string ReportAlreadyClosed = "This report has already been resolved or dismissed.";

    public const string ResolveActionInvalid = "A report is resolved with a Warn, Hide, or Remove action.";

    /// <summary>
    /// A Hide/Remove decision could not be applied to the real content in the
    /// owning service, so the report was deliberately left open.
    /// </summary>
    public const string ModerationApplyFailed =
        "The Hide/Remove decision could not be applied to the target content; the report was left open. Please retry.";

    public const string TargetIdRequired = "The reported target id is required.";
    public const string InvalidPageParameters = "Invalid pagination parameters.";
}
