namespace Notification.Application.Constants;

/// <summary>
/// Centralized business error messages surfaced through the standard error
/// envelope, per code-standard.md section 11.
/// </summary>
public static class ApplicationErrorConstants
{
    public const string CallerNotAuthenticated = "The request is not associated with an authenticated user.";

    public const string NotificationNotFound = "Notification not found.";
    public const string InvalidNotificationType = "Unknown notification type.";

    public const string TitleRequired = "Title is required.";
    public const string BodyRequired = "Body is required.";
    public const string RecipientRequired = "A recipient user id is required.";
    public const string RefTypeAndIdTogether = "RefType and RefId must be provided together.";
    public const string InvalidPageParameters = "Invalid pagination parameters.";
}
