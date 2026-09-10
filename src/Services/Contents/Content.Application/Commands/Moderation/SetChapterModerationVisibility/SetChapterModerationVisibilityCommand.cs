namespace Content.Application.Commands.Moderation.SetChapterModerationVisibility;

/// <summary>
/// Internal, service-to-service: applies a Moderation service Hide/Remove (or
/// restore) decision to a single chapter. Reached only through the
/// <c>X-Service-Token</c>-protected internal endpoint.
/// </summary>
public sealed class SetChapterModerationVisibilityCommand : ICommand<ModerationVisibilityResponseDto>
{
    public Guid ChapterId { get; init; }

    public bool Hidden { get; init; }

    public string Reason { get; init; }
}
