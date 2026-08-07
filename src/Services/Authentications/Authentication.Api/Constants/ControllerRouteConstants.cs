namespace Authentication.Api.Constants;

/// <summary>
/// Centralized route segments, per code-standard.md section 11 (Constants Rules)
/// and api-guidelines.md sections 3-4 (Versioning, URI Design).
/// </summary>
public static class ControllerRouteConstants
{
    public const string ApiVersion1 = "v1";
    public const string AuthBase = "v1/auth";

    public const string RegisterSegment = "register";
    public const string LoginSegment = "login";
    public const string MeSegment = "me";
}
