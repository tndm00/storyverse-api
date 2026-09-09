namespace Moderation.Domain.Enums;

/// <summary>
/// Why a user filed a <see cref="Entities.Report"/>, per
/// product-workflow-context.md section 4.
/// </summary>
public enum ReportReason
{
    Copyright,
    Inappropriate,
    Spam,
    Other
}
