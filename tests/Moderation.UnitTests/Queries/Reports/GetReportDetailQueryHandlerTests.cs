using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Moderation.Application.Interfaces.Repositories;
using Moderation.Application.Queries.Reports.GetReportDetail;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Moderation.UnitTests.Queries.Reports;

public class GetReportDetailQueryHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IModerationActionRepository _actionRepository = Substitute.For<IModerationActionRepository>();

    private readonly GetReportDetailQueryHandler _handler;

    public GetReportDetailQueryHandlerTests()
    {
        _handler = new GetReportDetailQueryHandler(_reportRepository, _actionRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnDetailWithActions_When_ReportExists()
    {
        var publicId = Guid.NewGuid();
        var report = new Report
        {
            Id = 7,
            PublicId = publicId,
            TargetType = ModerationTargetType.Chapter,
            TargetId = Guid.NewGuid(),
            Reason = ReportReason.Copyright,
            Status = ReportStatus.Reviewing
        };
        var action = new ModerationAction
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            ReportId = 7,
            ModeratorUserId = 3,
            TargetType = ModerationTargetType.Chapter,
            TargetId = report.TargetId,
            Action = ModerationActionType.Warn
        };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);
        _actionRepository.GetByReportIdAsync(7, Arg.Any<CancellationToken>()).Returns(new[] { action });

        var query = new GetReportDetailQuery { ReportId = publicId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(publicId);
        result.Actions.Should().ContainSingle(a => a.Id == action.PublicId);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_ReportMissing()
    {
        var publicId = Guid.NewGuid();
        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns((Report)null);

        var query = new GetReportDetailQuery { ReportId = publicId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
