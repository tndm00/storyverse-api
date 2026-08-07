namespace Authentication.Infrastructure.Constants;

public static class InfrastructureErrorConstants
{
    public const string DatabaseConnectionFailed = "Unable to reach the authentication database.";

    public const string UnauthenticatedUserContext = "Authenticated user context is missing or invalid.";
}
