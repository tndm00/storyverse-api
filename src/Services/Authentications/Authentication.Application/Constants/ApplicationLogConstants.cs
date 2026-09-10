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

    public const string RefreshAttempt = "Refresh token exchange attempt.";
    public const string RefreshSucceeded = "User {UserId} exchanged a refresh token for a new access token.";
    public const string RefreshFailedUnknownOrExpired = "Refresh failed: token unknown, expired, or for an inactive account.";
    public const string RefreshTokenReuseDetected =
        "Refresh token reuse detected for user {UserId}; revoked all active refresh tokens for the account.";
    public const string LogoutSucceeded = "User {UserId} revoked a refresh token (logout).";

    public const string GoogleLoginAttempt = "Google login attempt.";
    public const string GoogleLoginSucceeded = "User {UserId} logged in with Google.";
    public const string GoogleLoginFailedUnverifiedEmail = "Google login failed: token carried an unverified email.";
    public const string GoogleAccountLinked = "Linked Google identity to existing user {UserId}.";
    public const string GoogleUserProvisioned = "Provisioned a new user from Google sign-in for email {Email}.";

    public const string AuthorProfileCreateAttempt = "Author profile creation attempt for user {UserId}.";
    public const string AuthorProfileCreated = "Author profile {AuthorProfileId} created for user {UserId}.";
    public const string AuthorProfileCreateFailedExists = "Author profile creation failed: user {UserId} already has one.";

    public const string UserRoleGranted = "Admin granted role {Role} to user {UserId}.";
    public const string UserRoleRevoked = "Admin revoked role {Role} from user {UserId}.";
}
