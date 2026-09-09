namespace Community.Application.Constants;

/// <summary>
/// Community service application-layer values: business limits that are not
/// routes, log templates, or error messages.
/// </summary>
public static class ApplicationConstants
{
    public const int MinPageSize = 1;
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 20;

    public const int MinRatingScore = 1;
    public const int MaxRatingScore = 5;

    public const int MaxCommentLength = 5000;
    public const int MaxReviewTextLength = 5000;
}
