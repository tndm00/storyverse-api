namespace Moderation.Api.Constants;

/// <summary>
/// Centralized route segments, per code-standard.md section 11 (Constants Rules)
/// and api-guidelines.md sections 3-4 (Versioning, URI Design).
/// </summary>
public static class ControllerRouteConstants
{
    public const string ApiVersion1 = "v1";

    public const string ReportsBase = "v1/reports";

    public const string ReportByIdSegment = "{reportId:guid}";
    public const string ReportReviewSegment = "{reportId:guid}/review";
    public const string ReportResolveSegment = "{reportId:guid}/resolve";
    public const string ReportDismissSegment = "{reportId:guid}/dismiss";
}
