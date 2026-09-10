using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.RemoveChapter;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class RemoveChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<RemoveChapterCommandHandler> _logger =
        Substitute.For<ILogger<RemoveChapterCommandHandler>>();

    private readonly RemoveChapterCommandHandler _handler;

    public RemoveChapterCommandHandlerTests()
    {
        _handler = new RemoveChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_MarkChapterRemoved_When_ChapterIsPublished()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = chapterPublicId, Status = ChapterStatus.Published };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new RemoveChapterCommand { ChapterId = chapterPublicId };

        var result = await _handler.Handle(command, CancellationToken.None);

        chapter.Status.Should().Be(ChapterStatus.Removed);
        result.Status.Should().Be(ChapterStatus.Removed.ToString());
        _chapterRepository.Received(1).Update(chapter);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft)]
    [InlineData(ChapterStatus.Scheduled)]
    [InlineData(ChapterStatus.PendingReview)]
    [InlineData(ChapterStatus.InReview)]
    [InlineData(ChapterStatus.Rejected)]
    [InlineData(ChapterStatus.Removed)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_ChapterCannotBeRemoved(ChapterStatus status)
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = chapterPublicId, Status = status };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new RemoveChapterCommand { ChapterId = chapterPublicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
