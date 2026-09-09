using Be.StoryVerse.Core.Exceptions;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Application.Queries.Stories.GetStoryDetail;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Stories;

public class GetStoryDetailQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();

    private readonly GetStoryDetailQueryHandler _handler;

    public GetStoryDetailQueryHandlerTests()
    {
        _handler = new GetStoryDetailQueryHandler(_storyRepository, _authorContext);
    }

    private static Story CreateDraftStory(long authorProfileId, string slug)
    {
        return new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Slug = slug,
            AuthorProfileId = authorProfileId,
            Status = StoryStatus.Draft
        };
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_DraftStoryReadByAnonymousCaller()
    {
        var story = CreateDraftStory(authorProfileId: 10, "my-story");
        _storyRepository.GetBySlugAsync("my-story", Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);

        var query = new GetStoryDetailQuery { Slug = "my-story" };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_DraftStoryReadByNonOwnerAuthor()
    {
        var story = CreateDraftStory(authorProfileId: 10, "my-story");
        _storyRepository.GetBySlugAsync("my-story", Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(true);
        _authorContext.GetAuthorProfileId().Returns(99L);

        var query = new GetStoryDetailQuery { Slug = "my-story" };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnStory_When_OwnerReadsOwnDraftStory()
    {
        var story = CreateDraftStory(authorProfileId: 10, "my-story");
        _storyRepository.GetBySlugAsync("my-story", Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(true);
        _authorContext.GetAuthorProfileId().Returns(10L);
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);

        var query = new GetStoryDetailQuery { Slug = "my-story" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Id.Should().Be(story.PublicId);
        await _storyRepository.DidNotReceive().IncrementViewCountAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_IncrementViewCount_When_NonOwnerReadsPublishedStory()
    {
        var story = new Story
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            Slug = "published-story",
            AuthorProfileId = 10,
            Status = StoryStatus.Ongoing,
            ViewCount = 5
        };
        _storyRepository.GetBySlugAsync("published-story", Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);

        var query = new GetStoryDetailQuery { Slug = "published-story" };

        var result = await _handler.Handle(query, CancellationToken.None);

        await _storyRepository.Received(1).IncrementViewCountAsync(story.Id, Arg.Any<CancellationToken>());
        result.ViewCount.Should().Be(6);
    }

    [Fact]
    public async Task Handle_Should_NotIncrementViewCount_When_OwnerPreviewsOwnPublishedStory()
    {
        var story = new Story
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            Slug = "published-story",
            AuthorProfileId = 10,
            Status = StoryStatus.Ongoing,
            ViewCount = 5
        };
        _storyRepository.GetBySlugAsync("published-story", Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(true);
        _authorContext.GetAuthorProfileId().Returns(10L);
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);

        var query = new GetStoryDetailQuery { Slug = "published-story" };

        await _handler.Handle(query, CancellationToken.None);

        await _storyRepository.DidNotReceive().IncrementViewCountAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }
}
