namespace Content.Application.Dtos;

/// <summary>
/// Internal request body from the Moderation service to hide or restore a story
/// or chapter as the result of a report decision.
/// </summary>
public sealed class ModerationVisibilityRequestDto
{
    /// <summary>True to withhold the target from public view; false to restore it.</summary>
    public bool Hidden { get; init; }

    /// <summary>Free-text moderation reason, carried for the audit trail / logs.</summary>
    public string Reason { get; init; }
}

/// <summary>Internal response: the target's public id and its new status.</summary>
public sealed class ModerationVisibilityResponseDto
{
    public Guid Id { get; init; }

    public string Status { get; init; }
}

/// <summary>Internal batch title lookup entry (story or chapter).</summary>
public sealed class ContentTitleEntryDto
{
    public Guid Id { get; init; }

    public string Title { get; init; }
}
