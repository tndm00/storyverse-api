using System.Net.Http;
using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moderation.Application.Commands.Reports.ResolveReport;
using Moderation.Application.Interfaces.Http;
using Moderation.Application.Interfaces.Persistence;
using Moderation.Application.Interfaces.Repositories;
using Moderation.Application.Interfaces.Services;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Moderation.UnitTests.Commands.Reports;

public class ResolveReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IModerationActionRepository _actionRepository = Substitute.For<IModerationActionRepository>();
    private readonly IModerationUnitOfWork _unitOfWork = Substitute.For<IModerationUnitOfWork>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly IContentModerationClient _contentClient = Substitute.For<IContentModerationClient>();
    private readonly ICommunityModerationClient _communityClient = Substitute.For<ICommunityModerationClient>();
    private readonly ILogger<ResolveReportCommandHandler> _logger =
        Substitute.For<ILogger<ResolveReportCommandHandler>>();

    private readonly ResolveReportCommandHandler _handler;

    public ResolveReportCommandHandlerTests()
    {
        _handler = new ResolveReportCommandHandler(
            _reportRepository, _actionRepository, _unitOfWork, _currentUser,
            _contentClient, _communityClient, _logger);

        // Run the transactional callback inline so assertions can observe its effects.
        _unitOfWork
            .ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_ReportMissing()
    {
        var publicId = Guid.NewGuid();
        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns((Report)null);

        var command = new ResolveReportCommand { ReportId = publicId, Action = ModerationActionType.Warn };

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

        var command = new ResolveReportCommand { ReportId = publicId, Action = ModerationActionType.Hide };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_RecordActionAndCloseReport_When_ReportIsPending()
    {
        var publicId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var report = new Report
        {
            Id = 10,
            PublicId = publicId,
            TargetType = ModerationTargetType.Story,
            TargetId = targetId,
            Status = ReportStatus.Pending
        };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);
        _currentUser.GetUserId().Returns(8L);
        _actionRepository.GetByReportIdAsync(10, Arg.Any<CancellationToken>()).Returns(Array.Empty<ModerationAction>());

        var command = new ResolveReportCommand
        {
            ReportId = publicId,
            Action = ModerationActionType.Remove,
            Note = "  removed for policy violation  "
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        // Hide/Remove must be applied to the real content before the report closes.
        await _contentClient.Received(1).SetStoryVisibilityAsync(
            targetId, true, "removed for policy violation", Arg.Any<CancellationToken>());
        report.Status.Should().Be(ReportStatus.Resolved);
        report.ResolvedAt.Should().NotBeNull();
        result.Status.Should().Be(nameof(ReportStatus.Resolved));

        await _actionRepository.Received(1).AddAsync(
            Arg.Is<ModerationAction>(a =>
                a.ReportId == 10 &&
                a.ModeratorUserId == 8L &&
                a.Action == ModerationActionType.Remove &&
                a.Note == "removed for policy violation" &&
                a.TargetType == ModerationTargetType.Story &&
                a.TargetId == targetId),
            Arg.Any<CancellationToken>());
        _reportRepository.Received(1).Update(report);
        await _actionRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ResolveReport_When_ReportIsReviewing()
    {
        var publicId = Guid.NewGuid();
        var report = new Report
        {
            Id = 11,
            PublicId = publicId,
            TargetType = ModerationTargetType.Comment,
            TargetId = Guid.NewGuid(),
            Status = ReportStatus.Reviewing
        };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);
        _currentUser.GetUserId().Returns(2L);
        _actionRepository.GetByReportIdAsync(11, Arg.Any<CancellationToken>()).Returns(Array.Empty<ModerationAction>());

        var command = new ResolveReportCommand { ReportId = publicId, Action = ModerationActionType.Warn };

        var result = await _handler.Handle(command, CancellationToken.None);

        report.Status.Should().Be(ReportStatus.Resolved);
        result.Status.Should().Be(nameof(ReportStatus.Resolved));
    }

    [Fact]
    public async Task Handle_Should_NotResolveReport_When_ContentHideFails()
    {
        var publicId = Guid.NewGuid();
        var report = new Report
        {
            Id = 12,
            PublicId = publicId,
            TargetType = ModerationTargetType.Comment,
            TargetId = Guid.NewGuid(),
            Status = ReportStatus.Reviewing
        };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);
        _currentUser.GetUserId().Returns(5L);
        _communityClient
            .SetCommentVisibilityAsync(Arg.Any<Guid>(), true, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new HttpRequestException("boom"));

        var command = new ResolveReportCommand { ReportId = publicId, Action = ModerationActionType.Hide };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        report.Status.Should().Be(ReportStatus.Reviewing);
        await _actionRepository.DidNotReceive().AddAsync(Arg.Any<ModerationAction>(), Arg.Any<CancellationToken>());
    }
}
