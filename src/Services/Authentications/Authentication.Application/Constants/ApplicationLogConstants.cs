namespace Authentication.Application.Constants;

/// <summary>
/// Centralized structured-logging message templates. Never include tokens,
/// passwords, or secrets in these templates, per auth-guidelines.md section 15.
/// </summary>
public static class ApplicationLogConstants
{
    public const string RegisterAttempt = "Register attempt for email {Email}.";
    public const string RegisterSucceeded = "User {UserId} registered successfully.";
    public const string RegisterFailedEmailExists = "Register failed: email {Email} already registered.";

    public const string LoginAttempt = "Login attempt for email {Email}.";
    public const string LoginSucceeded = "User {UserId} logged in successfully.";
    public const string LoginFailedInvalidCredentials = "Login failed: invalid credentials for email {Email}.";
    public const string LoginFailedAccountNotActive = "Login failed: account {UserId} is not active.";
}
