namespace Authentication.Application.Constants;

/// <summary>
/// Business error messages returned to callers. Kept generic for authentication
/// failures per auth-guidelines.md section 11 (do not reveal whether an email exists).
/// </summary>
public static class ApplicationErrorConstants
{
    public const string EmailAlreadyRegistered = "Unable to complete registration with the provided details.";
    public const string InvalidCredentials = "Invalid email or password.";
    public const string AccountNotActive = "Invalid email or password.";
    public const string UserNotFound = "User was not found.";
    public const string UnauthenticatedRequest = "Authentication is required to access this resource.";

    public const string AuthorProfileAlreadyExists = "This account already has an author profile.";
    public const string AuthorProfileNotFound = "No author profile exists for this account.";

    /// <summary>Admin role management: the supplied role name is not a platform role.</summary>
    public const string InvalidRoleName = "Unknown role name. Valid roles: Reader, Author, Moderator, PlatformAdmin.";

    /// <summary>Admin role management: Reader is the implicit floor and cannot be revoked.</summary>
    public const string ReaderRoleCannotBeRevoked = "The Reader role is implicit and cannot be revoked.";

    /// <summary>Generic message for every Google sign-in failure — never says which check failed.</summary>
    public const string GoogleAuthFailed = "Unable to sign in with Google.";

    /// <summary>FluentValidation message template for pen name length; format with the max length.</summary>
    public const string PenNameLengthRequirementFormat = "Pen name is required and must be at most {0} characters.";

    /// <summary>
    /// FluentValidation message for an invalid email shape, per
    /// code-standard.md section 34 (Validation Rules).
    /// </summary>
    public const string InvalidEmailFormat = "A valid email address is required.";

    /// <summary>
    /// FluentValidation message template for password strength; format with
    /// <see cref="ApplicationConstants.MinPasswordLength"/>.
    /// </summary>
    public const string PasswordStrengthRequirementFormat =
        "Password must be at least {0} characters and include a letter and a digit.";
}
