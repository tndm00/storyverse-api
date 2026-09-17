namespace Authentication.Infrastructure.Constants;

/// <summary>
/// Error message literals raised by the Authentication.Infrastructure project.
/// </summary>
public static class InfrastructureErrorConstants
{
    public const string DatabaseConnectionFailed = "Unable to reach the authentication database.";

    public const string UnauthenticatedUserContext = "Authenticated user context is missing or invalid.";
}
