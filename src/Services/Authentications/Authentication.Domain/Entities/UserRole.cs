namespace Authentication.Domain.Entities;

/// <summary>
/// Grants one <see cref="Enums.Role"/> to one <see cref="User"/>. A join row with
/// a composite key (<see cref="UserId"/>, <see cref="Role"/>) — deliberately not
/// a <see cref="Be.StoryVerse.Core.Models.BaseEntity"/>, it has no identity of
/// its own.
/// </summary>
public sealed class UserRole
{
    public long UserId { get; set; }

    public Role Role { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}
