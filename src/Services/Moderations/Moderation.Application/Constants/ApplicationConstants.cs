namespace Moderation.Application.Constants;

/// <summary>
/// Moderation service application-layer values: business limits that are not
/// routes, log templates, or error messages, per code-standard.md section 11.
/// </summary>
public static class ApplicationConstants
{
    public const int MinPageSize = 1;
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 20;

    public const int MaxDescriptionLength = 2000;
    public const int MaxNoteLength = 2000;
}
