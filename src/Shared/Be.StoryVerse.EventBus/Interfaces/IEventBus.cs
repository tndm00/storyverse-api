namespace Be.StoryVerse.EventBus.Interfaces;

/// <summary>
/// Publishes integration events for asynchronous, decoupled workflows across
/// services, per code-standard.md section 47.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
