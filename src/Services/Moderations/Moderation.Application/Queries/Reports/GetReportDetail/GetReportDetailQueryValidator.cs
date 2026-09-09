namespace Moderation.Application.Queries.Reports.GetReportDetail;

public sealed class GetReportDetailQueryValidator : AbstractValidator<GetReportDetailQuery>
{
    public GetReportDetailQueryValidator()
    {
        RuleFor(x => x.ReportId).NotEmpty();
    }
}
