namespace Be.StoryVerse.Shared.Enums;

/// <summary>
/// Generic account/record lifecycle status reused across services when a
/// service-specific enum is not warranted. Authentication's <c>User</c> entity
/// uses its own domain enum (<c>Authentication.Domain.Enums.UserStatus</c>);
/// this shared enum exists for cross-service contracts that only need a
/// coarse active/suspended/deleted state.
/// </summary>
public enum AccountStatus
{
    Active = 0,
    Suspended = 1,
    Deleted = 2
}
