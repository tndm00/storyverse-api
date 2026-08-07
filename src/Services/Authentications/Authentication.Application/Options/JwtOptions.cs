namespace Authentication.Application.Options;

/// <summary>
/// Binds the <c>GcpSettings:AuthSettings</c> configuration section, per
/// code-standard.md section 42. The signing key must never be a committed
/// secret; provide it via environment variable or Secret Manager in every
/// environment beyond local development, per auth-guidelines.md section 12.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "GcpSettings:AuthSettings";

    public string Issuer { get; init; } = string.Empty;

    public string[] Audiences { get; init; } = Array.Empty<string>();

    public string SecretKey { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 15;

    public int RefreshTokenDays { get; init; } = 14;
}
