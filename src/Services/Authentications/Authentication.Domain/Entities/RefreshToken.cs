namespace Authentication.Domain.Entities;

/// <summary>
/// A persisted refresh token, per auth-guidelines.md section 8 (Token Rules).
/// Only a hash of the opaque token value is stored — the raw value is returned
/// to the client once and never retained. Rotation replaces one row with a new
/// one and stamps <see cref="ReplacedByTokenHash"/> so a re-used (already
/// rotated) token can be detected.
/// </summary>
public sealed class RefreshToken : BaseEntity
{
    public long UserId { get; set; }

    /// <summary>SHA-256 hash (hex) of the raw refresh token value.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the token is rotated out or explicitly revoked (logout / reuse detection).</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash of the token that replaced this one on rotation; null until rotated.</summary>
    public string ReplacedByTokenHash { get; set; }

    /// <summary>Usable only while not revoked and not past expiry.</summary>
    public bool IsActive(DateTime asOfUtc) => RevokedAt is null && ExpiresAt > asOfUtc;
}
