using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moderation.Application.Commands.Reports.DismissReport;
using Moderation.Application.Interfaces.Persistence;
using Moderation.Application.Interfaces.Repositories;
using Moderation.Application.Interfaces.Services;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Moderation.UnitTests.Commands.Reports;

public class DismissReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IModerationActionRepository _actionRepository = Substitute.For<IModerationActionRepository>();
    private readonly IModerationUnitOfWork _unitOfWork = Substitute.For<IModerationUnitOfWork>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<DismissReportCommandHandler> _logger =
        Substitute.For<ILogger<DismissReportCommandHandler>>();

    private readonly DismissReportCommandHandler _handler;

    public DismissReportCommandHandlerTests()
    {
        _handler = new DismissReportCommandHandler(
            _reportRepository, _actionRepository, _unitOfWork, _currentUser, _logger);

        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_ReportMissing()
    {
        var publicId = Guid.NewGuid();
        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns((Report)null);

        var command = new DismissReportCommand { ReportId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(ReportStatus.Resolved)]
    [InlineData(ReportStatus.Dismissed)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ReportAlreadyClosed(ReportStatus status)
    {
        var publicId = Guid.NewGuid();
        var report = new Report { Id = 1, PublicId = publicId, Status = status };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);

        var command = new DismissReportCommand { ReportId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_RecordDismissActionAndCloseReport_When_ReportIsPending()
    {
        var publicId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var report = new Report
        {
            Id = 20,
            PublicId = publicId,
            TargetType = ModerationTargetType.Chapter,
            TargetId = targetId,
            Status = ReportStatus.Pending
        };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);
        _currentUser.GetUserId().Returns(6L);
        _actionRepository.GetByReportIdAsync(20, Arg.Any<CancellationToken>()).Returns(Array.Empty<ModerationAction>());

        var command = new DismissReportCommand { ReportId = publicId, Note = "  not a violation  " };

        var result = await _handler.Handle(command, CancellationToken.None);

        report.Status.Should().Be(ReportStatus.Dismissed);
        report.ResolvedAt.Should().NotBeNull();
        result.Status.Should().Be(nameof(ReportStatus.Dismissed));

        await _actionRepository.Received(1).AddAsync(
            Arg.Is<ModerationAction>(a =>
                a.ReportId == 20 &&
                a.ModeratorUserId == 6L &&
                a.Action == ModerationActionType.Dismiss &&
                a.Note == "not a violation" &&
                a.TargetType == ModerationTargetType.Chapter &&
                a.TargetId == targetId),
            Arg.Any<CancellationToken>());
        _reportRepository.Received(1).Update(report);
        await _actionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_DismissReport_When_ReportIsReviewing()
    {
        var publicId = Guid.NewGuid();
        var report = new Report
        {
            Id = 21,
            PublicId = publicId,
            TargetType = ModerationTargetType.Story,
            TargetId = Guid.NewGuid(),
            Status = ReportStatus.Reviewing
        };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);
        _currentUser.GetUserId().Returns(4L);
        _actionRepository.GetByReportIdAsync(21, Arg.Any<CancellationToken>()).Returns(Array.Empty<ModerationAction>());

        var command = new DismissReportCommand { ReportId = publicId };

        var result = await _handler.Handle(command, CancellationToken.None);

        report.Status.Should().Be(ReportStatus.Dismissed);
        result.Status.Should().Be(nameof(ReportStatus.Dismissed));
    }
}
