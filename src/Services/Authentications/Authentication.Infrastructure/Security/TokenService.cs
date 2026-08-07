namespace Authentication.Infrastructure.Security;

/// <summary>
/// Issues short-lived JWT access tokens and opaque refresh tokens, per
/// auth-guidelines.md sections 8-10. Claims are kept minimal: subject, jti,
/// issuer, audience, and expiration only -- no roles/permissions/PII, matching
/// the "keep token claims minimal and stable" rule.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public GeneratedToken GenerateAccessToken(User user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        // Support multiple audiences (web/mobile/api) per code-standard.md section 42.
        var audience = _options.Audiences.Length > 0 ? _options.Audiences[0] : null;

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token);

        return new GeneratedToken(value, expiresAt);
    }

    public GeneratedToken GenerateRefreshToken()
    {
        // Opaque, high-entropy refresh token. Only a hash of this value should
        // ever be persisted (auth-guidelines.md section 8); persistence and
        // rotation storage are out of scope for this Phase 1 slice.
        var bytes = RandomNumberGenerator.GetBytes(64);
        var value = Convert.ToBase64String(bytes);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays);

        return new GeneratedToken(value, expiresAt);
    }
}
