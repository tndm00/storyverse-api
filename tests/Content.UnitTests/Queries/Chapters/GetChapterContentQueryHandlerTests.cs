using Be.StoryVerse.Cache.ViewTracking;
using Be.StoryVerse.Core.Exceptions;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Application.Queries.Chapters.GetChapterContent;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Chapters;

public class GetChapterContentQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly IViewTracker _viewTracker = Substitute.For<IViewTracker>();

    private readonly GetChapterContentQueryHandler _handler;

    public GetChapterContentQueryHandlerTests()
    {
        _handler = new GetChapterContentQueryHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _authorContext, _viewTracker);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_UnpublishedChapterReadByNonOwner()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = chapterPublicId, Status = ChapterStatus.Draft };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);

        var query = new GetChapterContentQuery { ChapterId = chapterPublicId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_IncrementViewCount_When_NonOwnerReadsPublishedChapter()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 1,
            StoryId = 5,
            PublicId = chapterPublicId,
            Status = ChapterStatus.Published,
            ViewCount = 7
        };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);

        var query = new GetChapterContentQuery { ChapterId = chapterPublicId };

        var result = await _handler.Handle(query, CancellationToken.None);

        await _viewTracker.Received(1).RecordChapterViewAsync(chapter.Id, story.Id, Arg.Any<CancellationToken>());
        result.ViewCount.Should().Be(8);
    }

    [Fact]
    public async Task Handle_Should_NotIncrementViewCount_When_OwnerReadsOwnPublishedChapter()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 1,
            StoryId = 5,
            PublicId = chapterPublicId,
            Status = ChapterStatus.Published,
            ViewCount = 7
        };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(true);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var query = new GetChapterContentQuery { ChapterId = chapterPublicId };

        var result = await _handler.Handle(query, CancellationToken.None);

        await _viewTracker.DidNotReceive()
            .RecordChapterViewAsync(Arg.Any<long>(), Arg.Any<long>(), Arg.Any<CancellationToken>());
        result.ViewCount.Should().Be(7);
    }
}
