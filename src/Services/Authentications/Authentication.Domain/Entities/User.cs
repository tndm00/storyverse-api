namespace Authentication.Domain.Entities;

/// <summary>
/// Login/auth identity owned by the Authentication service. This is not the
/// business/publishing identity: content ownership belongs to <c>AuthorProfile</c>
/// in the Content service (see product-workflow-context.md, "User vs AuthorProfile").
/// </summary>
public sealed class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password. Null when the account is OAuth-only.
    /// </summary>
    public string PasswordHash { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// External identity provider this account is linked to (e.g. <c>Google</c>),
    /// or null for a password-only account. An account may have both a password
    /// and a linked provider.
    /// </summary>
    public string ExternalProvider { get; set; }

    /// <summary>
    /// Stable subject identifier issued by <see cref="ExternalProvider"/> (the
    /// Google <c>sub</c> claim). Null when no provider is linked. Matched on
    /// before email so a provider re-using an email cannot take over an account.
    /// </summary>
    public string ExternalId { get; set; }

    /// <summary>
    /// Roles granted to this account. Every account is at least a
    /// <see cref="Enums.Role.Reader"/>; an author onboarding adds
    /// <see cref="Enums.Role.Author"/>.
    /// </summary>
    public ICollection<UserRole> Roles { get; } = new List<UserRole>();
}
