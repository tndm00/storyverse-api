using FluentAssertions;
using Moderation.Application.Constants;
using Moderation.Application.Dtos;
using Moderation.Application.Interfaces.Repositories;
using Moderation.Application.Queries.Reports.GetReports;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Moderation.UnitTests.Queries.Reports;

public class GetReportsQueryHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();

    private readonly GetReportsQueryHandler _handler;

    public GetReportsQueryHandlerTests()
    {
        _handler = new GetReportsQueryHandler(_reportRepository);
    }

    [Fact]
    public async Task Handle_Should_ParseStatusFilter_When_StatusProvided()
    {
        _reportRepository
            .SearchAsync(Arg.Any<ReportSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Report>(), 0));

        var query = new GetReportsQuery { Status = "Pending" };

        await _handler.Handle(query, CancellationToken.None);

        await _reportRepository.Received(1).SearchAsync(
            Arg.Is<ReportSearchCriteria>(c => c.Status == ReportStatus.Pending),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ParseReasonFilter_When_ReasonProvided()
    {
        _reportRepository
            .SearchAsync(Arg.Any<ReportSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Report>(), 0));

        var query = new GetReportsQuery { Reason = "Spam" };

        await _handler.Handle(query, CancellationToken.None);

        await _reportRepository.Received(1).SearchAsync(
            Arg.Is<ReportSearchCriteria>(c => c.Reason == ReportReason.Spam),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_LeaveFiltersNull_When_StatusAndReasonAreInvalid()
    {
        _reportRepository
            .SearchAsync(Arg.Any<ReportSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Report>(), 0));

        var query = new GetReportsQuery { Status = "NotAStatus", Reason = "NotAReason" };

        await _handler.Handle(query, CancellationToken.None);

        await _reportRepository.Received(1).SearchAsync(
            Arg.Is<ReportSearchCriteria>(c => c.Status == null && c.Reason == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ClampPageSize_When_PageSizeExceedsMax()
    {
        _reportRepository
            .SearchAsync(Arg.Any<ReportSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Report>(), 0));

        var query = new GetReportsQuery { PageSize = 500 };

        await _handler.Handle(query, CancellationToken.None);

        await _reportRepository.Received(1).SearchAsync(
            Arg.Is<ReportSearchCriteria>(c => c.PageSize == ApplicationConstants.MaxPageSize),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnPagedResponse_When_ItemsExist()
    {
        var report = new Report
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            TargetType = ModerationTargetType.Story,
            TargetId = Guid.NewGuid(),
            Reason = ReportReason.Inappropriate,
            Status = ReportStatus.Pending
        };

        _reportRepository
            .SearchAsync(Arg.Any<ReportSearchCriteria>(), Arg.Any<CancellationToken>())
            .Returns((new[] { report }, 1));

        var query = new GetReportsQuery { PageNumber = 1, PageSize = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle(i => i.Id == report.PublicId);
    }
}
