namespace Content.Application.Commands.Moderation.SetStoryModerationVisibility;

/// <summary>
/// Internal, service-to-service: applies a Moderation service Hide/Remove (or
/// restore) decision to a story. Not an author action — reached only through the
/// <c>X-Service-Token</c>-protected internal endpoint.
/// </summary>
public sealed class SetStoryModerationVisibilityCommand : ICommand<ModerationVisibilityResponseDto>
{
    public Guid StoryId { get; init; }

    public bool Hidden { get; init; }

    public string Reason { get; init; }
}
