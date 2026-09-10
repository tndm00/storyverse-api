using Be.StoryVerse.Core.Exceptions;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Application.Queries.Chapters.GetStoryChapters;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Chapters;

public class GetStoryChaptersQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();

    private readonly GetStoryChaptersQueryHandler _handler;

    public GetStoryChaptersQueryHandlerTests()
    {
        _handler = new GetStoryChaptersQueryHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _authorContext);
        _volumeRepository.GetByStoryAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(new List<Volume>());
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_DraftStoryReadByNonOwner()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10, Status = StoryStatus.Draft };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);

        var query = new GetStoryChaptersQuery { StoryId = story.PublicId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_RequestPublishedOnlyChapters_When_CallerIsNotOwner()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10, Status = StoryStatus.Ongoing };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);
        _chapterRepository.GetByStoryAsync(1, publishedOnly: true, Arg.Any<CancellationToken>())
            .Returns(new List<Chapter>());

        var query = new GetStoryChaptersQuery { StoryId = story.PublicId };

        await _handler.Handle(query, CancellationToken.None);

        await _chapterRepository.Received(1).GetByStoryAsync(1, publishedOnly: true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RequestAllChapters_When_CallerIsOwner()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10, Status = StoryStatus.Draft };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(true);
        _authorContext.GetAuthorProfileId().Returns(10L);
        _chapterRepository.GetByStoryAsync(1, publishedOnly: false, Arg.Any<CancellationToken>())
            .Returns(new List<Chapter>());

        var query = new GetStoryChaptersQuery { StoryId = story.PublicId };

        await _handler.Handle(query, CancellationToken.None);

        await _chapterRepository.Received(1).GetByStoryAsync(1, publishedOnly: false, Arg.Any<CancellationToken>());
    }
}
