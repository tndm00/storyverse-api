namespace Moderation.Application.Dtos;

/// <summary>Payload any authenticated user sends to file a report.</summary>
public sealed class SubmitReportRequestDto
{
    public ModerationTargetType TargetType { get; init; }

    public Guid TargetId { get; init; }

    public ReportReason Reason { get; init; }

    public string Description { get; init; }
}

/// <summary>Payload a moderator sends to resolve a report with an action against the content.</summary>
public sealed class ResolveReportRequestDto
{
    /// <summary>One of <c>Warn</c>, <c>Hide</c>, <c>Remove</c>. <c>Dismiss</c> uses the dismiss endpoint.</summary>
    public ModerationActionType Action { get; init; }

    public string Note { get; init; }
}

/// <summary>Payload a moderator sends to dismiss a report with no action against the content.</summary>
public sealed class DismissReportRequestDto
{
    public string Note { get; init; }
}

/// <summary>Report row in the moderation queue listing.</summary>
public sealed class ReportSummaryResponseDto
{
    public Guid Id { get; init; }

    public string TargetType { get; init; }

    public Guid TargetId { get; init; }

    /// <summary>
    /// Reported target's title (story/chapter) or comment excerpt, resolved from
    /// the owning service. Null when the lookup was unavailable — clients fall
    /// back to <c>"&lt;TargetType&gt; &lt;id8&gt;"</c>.
    /// </summary>
    public string TargetTitle { get; init; }

    public long ReporterUserId { get; init; }

    /// <summary>Reporter's display name from Authentication. Null on lookup failure.</summary>
    public string ReporterDisplayName { get; init; }

    public string Reason { get; init; }

    public string Status { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ResolvedAt { get; init; }
}

/// <summary>Full report detail, including the moderation actions recorded against it.</summary>
public sealed class ReportDetailResponseDto
{
    public Guid Id { get; init; }

    public long ReporterUserId { get; init; }

    /// <summary>Reporter's display name from Authentication. Null on lookup failure.</summary>
    public string ReporterDisplayName { get; init; }

    public string TargetType { get; init; }

    public Guid TargetId { get; init; }

    /// <summary>Reported target's title / comment excerpt from the owning service. Null on lookup failure.</summary>
    public string TargetTitle { get; init; }

    public string Reason { get; init; }

    public string Description { get; init; }

    public string Status { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public DateTime? ResolvedAt { get; init; }

    public IReadOnlyList<ModerationActionResponseDto> Actions { get; init; } = Array.Empty<ModerationActionResponseDto>();
}

/// <summary>A single audit entry from the moderation trail.</summary>
public sealed class ModerationActionResponseDto
{
    public Guid Id { get; init; }

    public long ModeratorUserId { get; init; }

    public string TargetType { get; init; }

    public Guid TargetId { get; init; }

    public string Action { get; init; }

    public string Note { get; init; }

    public DateTime CreatedAt { get; init; }
}
