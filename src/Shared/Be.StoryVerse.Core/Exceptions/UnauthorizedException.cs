namespace Be.StoryVerse.Core.Exceptions;

/// <summary>
/// The request could not be authenticated: a missing, malformed, expired, or
/// revoked credential (for example a refresh token that no longer grants a
/// session). Maps to HTTP 401 by the global exception handler.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
