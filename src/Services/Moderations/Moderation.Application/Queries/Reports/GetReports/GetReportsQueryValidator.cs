namespace Moderation.Application.Queries.Reports.GetReports;

/// <summary>
/// Page size is clamped (not rejected) by the handler, so only the page number
/// lower bound is enforced here.
/// </summary>
public sealed class GetReportsQueryValidator : AbstractValidator<GetReportsQuery>
{
    /// <summary>Requires the page number to be positive.</summary>
    public GetReportsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(ApplicationErrorConstants.InvalidPageParameters);
    }
}
