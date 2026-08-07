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
    public string? PasswordHash { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }
}
