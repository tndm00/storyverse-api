namespace Be.StoryVerse.EventBus.Events;

/// <summary>
/// Base contract for events published across service boundaries via the
/// event bus. Concrete events (for example <c>UserRegisteredIntegrationEvent</c>)
/// live in the owning service.
/// </summary>
public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
