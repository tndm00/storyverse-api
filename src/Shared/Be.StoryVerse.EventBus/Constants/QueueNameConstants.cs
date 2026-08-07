namespace Be.StoryVerse.EventBus.Constants;

/// <summary>
/// Centralized queue/topic names for event bus publishers and subscribers.
/// </summary>
public static class QueueNameConstants
{
    public const string UserRegistered = "storyverse.authentication.user-registered";
    public const string UserLoggedIn = "storyverse.authentication.user-logged-in";
}
