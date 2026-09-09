namespace Moderation.Application.Mappings;

/// <summary>
/// Explicit entity-to-response-DTO projection for the Moderation service.
/// Entities are never returned directly as API contracts (code-standard.md
/// section 19); this is the single place that translates them.
/// </summary>
public static class ModerationDtoMapper
{
    public static ReportSummaryResponseDto ToSummary(Report report)
    {
        return new ReportSummaryResponseDto
        {
            Id = report.PublicId,
            TargetType = report.TargetType.ToString(),
            TargetId = report.TargetId,
            Reason = report.Reason.ToString(),
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            ResolvedAt = report.ResolvedAt
        };
    }

    public static ReportDetailResponseDto ToDetail(Report report, IReadOnlyList<ModerationAction> actions)
    {
        return new ReportDetailResponseDto
        {
            Id = report.PublicId,
            ReporterUserId = report.ReporterUserId,
            TargetType = report.TargetType.ToString(),
            TargetId = report.TargetId,
            Reason = report.Reason.ToString(),
            Description = report.Description,
            Status = report.Status.ToString(),
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            ResolvedAt = report.ResolvedAt,
            Actions = actions.Select(ToDto).ToArray()
        };
    }

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
