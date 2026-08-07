namespace Authentication.Domain.Enums;

/// <summary>
/// Lifecycle status of a login/auth identity (<see cref="Entities.User"/>).
/// </summary>
public enum UserStatus
{
    Active = 0,
    Suspended = 1,
    Deleted = 2
}
