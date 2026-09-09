using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.RejectChapter;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class RejectChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _currentUser = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<RejectChapterCommandHandler> _logger =
        Substitute.For<ILogger<RejectChapterCommandHandler>>();

    private readonly RejectChapterCommandHandler _handler;

    public RejectChapterCommandHandlerTests()
    {
        _handler = new RejectChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _currentUser, _logger);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft)]
    [InlineData(ChapterStatus.PendingReview)]
    [InlineData(ChapterStatus.Published)]
    [InlineData(ChapterStatus.Rejected)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ChapterIsNotInReview(ChapterStatus status)
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = publicId, Status = status };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);

        var command = new RejectChapterCommand { ChapterId = publicId, Reason = "Not good enough" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_TrimAndStoreReason_When_Rejecting()
    {
        var publicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = publicId, Status = ChapterStatus.InReview };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid() };

        _chapterRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);

        var command = new RejectChapterCommand { ChapterId = publicId, Reason = "  Needs more detail  " };

        await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Rejected);
        chapter.RejectionReason.Should().Be("Needs more detail");
    }
}
