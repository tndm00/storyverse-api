namespace Community.Application.Dtos;

/// <summary>
/// Internal request body from the Moderation service to hide or restore a
/// comment as the result of a report decision.
/// </summary>
public sealed class ModerationVisibilityRequestDto
{
    /// <summary>True to hide the comment; false to restore it to visible.</summary>
    public bool Hidden { get; init; }

    /// <summary>Free-text moderation reason, carried for logs / the audit trail.</summary>
    public string Reason { get; init; }
}
