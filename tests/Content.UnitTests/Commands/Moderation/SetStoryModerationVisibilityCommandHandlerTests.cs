using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Moderation.SetStoryModerationVisibility;
using Content.Application.Interfaces.Repositories;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Moderation;

public class SetStoryModerationVisibilityCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly SetStoryModerationVisibilityCommandHandler _handler;

    public SetStoryModerationVisibilityCommandHandlerTests()
    {
        _handler = new SetStoryModerationVisibilityCommandHandler(
            _storyRepository, Substitute.For<ILogger<SetStoryModerationVisibilityCommandHandler>>());
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFound_When_StoryMissing()
    {
        _storyRepository.GetByPublicIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Story)null);

        Func<Task> act = () => _handler.Handle(
            new SetStoryModerationVisibilityCommand { StoryId = Guid.NewGuid(), Hidden = true }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_SetRemoved_When_Hiding()
    {
        var id = Guid.NewGuid();
        var story = new Story { PublicId = id, Status = StoryStatus.Ongoing };
        _storyRepository.GetByPublicIdAsync(id, Arg.Any<CancellationToken>()).Returns(story);

        var result = await _handler.Handle(
            new SetStoryModerationVisibilityCommand { StoryId = id, Hidden = true }, CancellationToken.None);

        story.Status.Should().Be(StoryStatus.Removed);
        result.Status.Should().Be("Removed");
    }

    [Fact]
    public async Task Handle_Should_RestoreToOngoing_When_UnhidingPublishedStory()
    {
        var id = Guid.NewGuid();
        var story = new Story { PublicId = id, Status = StoryStatus.Removed, PublishedAt = DateTime.UtcNow };
        _storyRepository.GetByPublicIdAsync(id, Arg.Any<CancellationToken>()).Returns(story);

        var result = await _handler.Handle(
            new SetStoryModerationVisibilityCommand { StoryId = id, Hidden = false }, CancellationToken.None);

        story.Status.Should().Be(StoryStatus.Ongoing);
        result.Status.Should().Be("Ongoing");
    }
}
