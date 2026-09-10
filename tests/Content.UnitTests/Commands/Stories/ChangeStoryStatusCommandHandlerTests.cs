using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Stories.ChangeStoryStatus;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class ChangeStoryStatusCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<ChangeStoryStatusCommandHandler> _logger =
        Substitute.For<ILogger<ChangeStoryStatusCommandHandler>>();

    private readonly ChangeStoryStatusCommandHandler _handler;

    public ChangeStoryStatusCommandHandlerTests()
    {
        _handler = new ChangeStoryStatusCommandHandler(_storyRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ChangeStatus_When_TransitionIsAllowedByPolicy()
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = StoryStatus.Ongoing
        };
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new ChangeStoryStatusCommand { StoryId = story.PublicId, TargetStatus = StoryStatus.Hiatus };

        var result = await _handler.Handle(command, CancellationToken.None);

        story.Status.Should().Be(StoryStatus.Hiatus);
        result.Status.Should().Be(StoryStatus.Hiatus.ToString());
        _storyRepository.Received(1).Update(story);
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessRuleException_When_TransitionIsNotAllowedByPolicy()
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            Status = StoryStatus.Draft
        };
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new ChangeStoryStatusCommand { StoryId = story.PublicId, TargetStatus = StoryStatus.Completed };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
