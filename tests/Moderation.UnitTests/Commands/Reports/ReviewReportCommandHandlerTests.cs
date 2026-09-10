using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moderation.Application.Commands.Reports.ReviewReport;
using Moderation.Application.Interfaces.Repositories;
using Moderation.Application.Interfaces.Services;
using Moderation.Domain.Entities;
using Moderation.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Moderation.UnitTests.Commands.Reports;

public class ReviewReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IModerationActionRepository _actionRepository = Substitute.For<IModerationActionRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<ReviewReportCommandHandler> _logger =
        Substitute.For<ILogger<ReviewReportCommandHandler>>();

    private readonly ReviewReportCommandHandler _handler;

    public ReviewReportCommandHandlerTests()
    {
        _handler = new ReviewReportCommandHandler(_reportRepository, _actionRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_ReportMissing()
    {
        var publicId = Guid.NewGuid();
        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns((Report)null);

        var command = new ReviewReportCommand { ReportId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData(ReportStatus.Reviewing)]
    [InlineData(ReportStatus.Resolved)]
    [InlineData(ReportStatus.Dismissed)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ReportIsNotPending(ReportStatus status)
    {
        var publicId = Guid.NewGuid();
        var report = new Report { Id = 1, PublicId = publicId, Status = status };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);

        var command = new ReviewReportCommand { ReportId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_SetStatusToReviewing_When_ReportIsPending()
    {
        var publicId = Guid.NewGuid();
        var report = new Report { Id = 1, PublicId = publicId, Status = ReportStatus.Pending };

        _reportRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(report);
        _actionRepository.GetByReportIdAsync(1, Arg.Any<CancellationToken>()).Returns(Array.Empty<ModerationAction>());
        _currentUser.GetUserId().Returns(5L);

        var command = new ReviewReportCommand { ReportId = publicId };

        var result = await _handler.Handle(command, CancellationToken.None);

        report.Status.Should().Be(ReportStatus.Reviewing);
        result.Status.Should().Be(nameof(ReportStatus.Reviewing));
        _reportRepository.Received(1).Update(report);
        await _reportRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
