namespace Moderation.Application.Queries.Reports.GetReportDetail;

/// <summary>Validation rules for <see cref="GetReportDetailQuery"/>.</summary>
public sealed class GetReportDetailQueryValidator : AbstractValidator<GetReportDetailQuery>
{
    /// <summary>Requires a non-empty report id.</summary>
    public GetReportDetailQueryValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
    }
}
