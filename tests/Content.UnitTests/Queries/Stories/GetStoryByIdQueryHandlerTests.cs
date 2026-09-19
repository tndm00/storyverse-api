using Be.StoryVerse.Cache.ViewTracking;
using Be.StoryVerse.Core.Exceptions;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Application.Queries.Stories.GetStoryById;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Stories;

public class GetStoryByIdQueryHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly IViewTracker _viewTracker = Substitute.For<IViewTracker>();

    private readonly GetStoryByIdQueryHandler _handler;

    public GetStoryByIdQueryHandlerTests()
    {
        _handler = new GetStoryByIdQueryHandler(_storyRepository, _authorContext, _viewTracker);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_DraftStoryReadByNonOwner()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10, Status = StoryStatus.Draft };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);

        var query = new GetStoryByIdQuery { StoryId = story.PublicId };

        Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ReturnDraftWithoutViewCount_When_CallerHasContentModeratePermission()
    {
        var story = new Story { Id = 3, PublicId = Guid.NewGuid(), AuthorProfileId = 10, Status = StoryStatus.Draft };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);
        _authorContext.HasPermission(Be.StoryVerse.Shared.Authorization.StoryVersePermissions.Content.Moderate)
            .Returns(true);

        var result = await _handler.Handle(new GetStoryByIdQuery { StoryId = story.PublicId }, CancellationToken.None);

        result.Status.Should().Be("Draft");
        await _viewTracker.DidNotReceive().RecordStoryViewAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_IncrementViewCount_When_NonOwnerReadsPublishedStory()
    {
        var story = new Story
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = StoryStatus.Ongoing,
            ViewCount = 3
        };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(false);
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);

        var query = new GetStoryByIdQuery { StoryId = story.PublicId };

        var result = await _handler.Handle(query, CancellationToken.None);

        await _viewTracker.Received(1).RecordStoryViewAsync(story.Id, Arg.Any<CancellationToken>());
        result.ViewCount.Should().Be(4);
    }

    [Fact]
    public async Task Handle_Should_NotIncrementViewCount_When_OwnerReadsOwnStory()
    {
        var story = new Story
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = StoryStatus.Ongoing,
            ViewCount = 3
        };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.IsAuthor.Returns(true);
        _authorContext.GetAuthorProfileId().Returns(10L);
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);

        var query = new GetStoryByIdQuery { StoryId = story.PublicId };

        await _handler.Handle(query, CancellationToken.None);

        await _viewTracker.DidNotReceive().RecordStoryViewAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }
}
