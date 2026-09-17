namespace Community.Application.Dtos;

/// <summary>
/// Normalizes raw <c>page-number</c> / <c>page-size</c> query values to safe
/// bounds, per api-guidelines.md section 6 (validate values).
/// </summary>
public static class PagingParameters
{
    /// <summary>Clamps a raw page number/size pair into safe bounds (page number at least 1, size within the configured min/max).</summary>
    public static (int PageNumber, int PageSize) Normalize(int pageNumber, int pageSize)
    {
        var normalizedNumber = Math.Max(1, pageNumber);
        var normalizedSize = Math.Clamp(
            pageSize <= 0 ? ApplicationConstants.DefaultPageSize : pageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        return (normalizedNumber, normalizedSize);
    }
}
