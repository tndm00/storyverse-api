namespace Moderation.Application.Mappings;

/// <summary>
/// Explicit entity-to-response-DTO projection for the Moderation service.
/// Entities are never returned directly as API contracts (code-standard.md
/// section 19); this is the single place that translates them.
/// </summary>
public static class ModerationDtoMapper
{
    /// <summary>Projects a report entity into a queue-listing summary DTO.</summary>
    public static ReportSummaryResponseDto ToSummary(
        Report report, string reporterDisplayName = null, string targetTitle = null)
    {
        return new ReportSummaryResponseDto
        {
            Id = report.PublicId,
            TargetType = report.TargetType.ToString(),
            TargetId = report.TargetId,
            TargetTitle = targetTitle,
            ReporterUserId = report.ReporterUserId,
            ReporterDisplayName = reporterDisplayName,
            Reason = report.Reason.ToString(),
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            ResolvedAt = report.ResolvedAt
        };
    }

    /// <summary>Projects a report entity, along with its recorded moderation actions, into a full detail DTO.</summary>
    public static ReportDetailResponseDto ToDetail(
        Report report,
        IReadOnlyList<ModerationAction> actions,
        string reporterDisplayName = null,
        string targetTitle = null)
    {
        return new ReportDetailResponseDto
        {
            Id = report.PublicId,
            ReporterUserId = report.ReporterUserId,
            ReporterDisplayName = reporterDisplayName,
            TargetType = report.TargetType.ToString(),
            TargetId = report.TargetId,
            TargetTitle = targetTitle,
            Reason = report.Reason.ToString(),
            Description = report.Description,
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            ResolvedAt = report.ResolvedAt,
            Actions = actions.Select(ToDto).ToArray()
        };
    }

    /// <summary>Projects a single moderation action entity into its response DTO.</summary>
    public static ModerationActionResponseDto ToDto(ModerationAction action)
    {
        return new ModerationActionResponseDto
        {
            Id = action.PublicId,
            ModeratorUserId = action.ModeratorUserId,
            TargetType = action.TargetType.ToString(),
            TargetId = action.TargetId,
            Action = action.Action.ToString(),
            Note = action.Note,
            CreatedAt = action.CreatedAt
        };
    }
}
