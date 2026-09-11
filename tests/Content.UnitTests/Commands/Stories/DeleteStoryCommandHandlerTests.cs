using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Stories.DeleteStory;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class DeleteStoryCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<DeleteStoryCommandHandler> _logger =
        Substitute.For<ILogger<DeleteStoryCommandHandler>>();

    private readonly DeleteStoryCommandHandler _handler;

    public DeleteStoryCommandHandlerTests()
    {
        _handler = new DeleteStoryCommandHandler(_storyRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_RemoveStory_When_DraftAndCallerIsOwner()
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = StoryStatus.Draft,
            // Cascade delete of chapters/volumes/story_genres/story_tags is
            // enforced at the database level (StoryConfiguration /
            // ChapterConfiguration / VolumeConfiguration all configure
            // DeleteBehavior.Cascade on the StoryId FK), so a single
            // Remove(story) + SaveChanges is sufficient here.
            Genres = new List<StoryGenre> { new StoryGenre { StoryId = 1, GenreId = 5 } },
            Tags = new List<StoryTag> { new StoryTag { StoryId = 1, TagId = 7 } }
        };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.HasPermission(Arg.Any<string>()).Returns(false);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new DeleteStoryCommand { StoryId = story.PublicId };

        await _handler.Handle(command, CancellationToken.None);

        _storyRepository.Received(1).Remove(story);
        await _storyRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_RemoveStory_When_DraftAndCallerHasModeratePermission_EvenIfNotOwner()
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = StoryStatus.Draft
        };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.HasPermission(Arg.Any<string>()).Returns(true);

        var command = new DeleteStoryCommand { StoryId = story.PublicId };

        await _handler.Handle(command, CancellationToken.None);

        _storyRepository.Received(1).Remove(story);
        _authorContext.DidNotReceive().GetAuthorProfileId();
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_StoryDoesNotExist()
    {
        _storyRepository.GetByPublicIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Story)null);

        var command = new DeleteStoryCommand { StoryId = Guid.NewGuid() };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _storyRepository.DidNotReceive().Remove(Arg.Any<Story>());
    }

    [Theory]
    [InlineData(StoryStatus.Ongoing)]
    [InlineData(StoryStatus.Completed)]
    [InlineData(StoryStatus.Hiatus)]
    [InlineData(StoryStatus.Dropped)]
    public async Task Handle_Should_ThrowBusinessRuleException_When_StatusIsNotDraft(StoryStatus status)
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = status
        };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);

        var command = new DeleteStoryCommand { StoryId = story.PublicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
        _storyRepository.DidNotReceive().Remove(Arg.Any<Story>());
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_When_DraftButCallerIsNotOwnerOrModerator()
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = StoryStatus.Draft
        };
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.HasPermission(Arg.Any<string>()).Returns(false);
        _authorContext.GetAuthorProfileId().Returns(99L);

        var command = new DeleteStoryCommand { StoryId = story.PublicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
        _storyRepository.DidNotReceive().Remove(Arg.Any<Story>());
    }
}
