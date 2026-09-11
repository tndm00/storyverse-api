namespace Content.Application.Dtos;

/// <summary>
/// Internal request body from the Community service: the recomputed rating
/// aggregate for a story, sent after a rating is upserted (best-effort sync,
/// not part of the Community write transaction).
/// </summary>
public sealed class RatingSummaryRequestDto
{
    public decimal RatingAvg { get; init; }

    public int RatingCount { get; init; }
}

/// <summary>Internal response: the story's public id and the stored rating summary.</summary>
public sealed class RatingSummaryResponseDto
{
    public Guid Id { get; init; }

    public decimal RatingAvg { get; init; }

    public int RatingCount { get; init; }
}
