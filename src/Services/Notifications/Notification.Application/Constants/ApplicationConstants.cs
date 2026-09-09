namespace Notification.Application.Constants;

/// <summary>
/// Notification service application-layer values: limits that are not routes,
/// log templates, or error messages, per code-standard.md section 11.
/// </summary>
public static class ApplicationConstants
{
    public const int MinPageSize = 1;
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 20;

    public const int MaxTitleLength = 200;
    public const int MaxBodyLength = 4000;
    public const int MaxRefTypeLength = 60;
}
