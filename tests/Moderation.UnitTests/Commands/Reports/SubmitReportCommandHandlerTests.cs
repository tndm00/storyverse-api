using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moderation.Application.Commands.Reports.SubmitReport;
using Moderation.Application.Interfaces.Repositories;
using Moderation.Application.Interfaces.Services;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Moderation.UnitTests.Commands.Reports;

public class SubmitReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<SubmitReportCommandHandler> _logger =
        Substitute.For<ILogger<SubmitReportCommandHandler>>();

    private readonly SubmitReportCommandHandler _handler;

    public SubmitReportCommandHandlerTests()
    {
        _handler = new SubmitReportCommandHandler(_reportRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_CreateReportAsPending_When_Submitted()
    {
        _currentUser.GetUserId().Returns(42L);

        var targetId = Guid.NewGuid();
        var command = new SubmitReportCommand
        {
            TargetType = ModerationTargetType.Chapter,
            TargetId = targetId,
            Reason = ReportReason.Spam,
            Description = "  spam content  "
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TargetType.Should().Be(nameof(ModerationTargetType.Chapter));
        result.TargetId.Should().Be(targetId);
        result.Reason.Should().Be(nameof(ReportReason.Spam));
        result.Status.Should().Be(nameof(ReportStatus.Pending));
        result.Description.Should().Be("spam content");
        await _reportRepository.Received(1).AddAsync(
            Arg.Is<Report>(r => r.ReporterUserId == 42L && r.Status == ReportStatus.Pending),
            Arg.Any<CancellationToken>());
        await _reportRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_SetDescriptionToNull_When_DescriptionIsWhitespace()
    {
        _currentUser.GetUserId().Returns(1L);

        var command = new SubmitReportCommand
        {
            TargetType = ModerationTargetType.Story,
            TargetId = Guid.NewGuid(),
            Reason = ReportReason.Other,
            Description = "   "
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Description.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_UseCurrentUserAsReporter_When_Submitted()
    {
        _currentUser.GetUserId().Returns(99L);

        var command = new SubmitReportCommand
        {
            TargetType = ModerationTargetType.Comment,
            TargetId = Guid.NewGuid(),
            Reason = ReportReason.Copyright,
            Description = null
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ReporterUserId.Should().Be(99L);
        result.Actions.Should().BeEmpty();
    }
}
