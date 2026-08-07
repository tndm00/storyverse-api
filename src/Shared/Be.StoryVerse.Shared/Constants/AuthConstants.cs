namespace Be.StoryVerse.Shared.Constants;

/// <summary>
/// Stable auth-scheme constants reused across services and layers (Api Swagger
/// wiring, Application-layer token response shaping, and any future service's
/// auth setup), per code-standard.md section 11 (Constants Rules) Placement
/// table — "Shared headers, base error codes, common response constants".
/// </summary>
public static class AuthConstants
{
    public const string BearerScheme = "Bearer";
}
