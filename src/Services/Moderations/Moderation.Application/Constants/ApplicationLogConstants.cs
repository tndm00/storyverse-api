namespace Moderation.Application.Constants;

/// <summary>
/// Structured-logging message templates for the Moderation service. No PII or
/// full report descriptions in these templates, per code-standard.md section 12.
/// </summary>
public static class ApplicationLogConstants
{
    public const string ReportSubmitted = "Report {ReportId} submitted against {TargetType} {TargetId}.";
    public const string ReportPickedUp = "Report {ReportId} moved to Reviewing by moderator {ModeratorUserId}.";
    public const string ReportResolved = "Report {ReportId} resolved with action {Action} by moderator {ModeratorUserId} (moderation action {ModerationActionId}).";
    public const string ReportDismissed = "Report {ReportId} dismissed by moderator {ModeratorUserId} (moderation action {ModerationActionId}).";
}
