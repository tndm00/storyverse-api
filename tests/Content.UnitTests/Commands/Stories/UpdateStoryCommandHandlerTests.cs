using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Stories.UpdateStory;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class UpdateStoryCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<UpdateStoryCommandHandler> _logger =
        Substitute.For<ILogger<UpdateStoryCommandHandler>>();

    private readonly UpdateStoryCommandHandler _handler;

    public UpdateStoryCommandHandlerTests()
    {
        _handler = new UpdateStoryCommandHandler(_storyRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_When_CallerIsNotStoryOwner()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.GetAuthorProfileId().Returns(99L);

        var command = new UpdateStoryCommand
        {
            StoryId = story.PublicId,
            Title = "Updated Title",
            Description = "Updated description"
        };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_Should_ClearOriginalSource_When_ContentTypeIsNotTranslated()
    {
        var story = new Story
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            AuthorProfileId = 10,
            ContentType = StoryContentType.Translated,
            OriginalSource = "Some Source"
        };
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new UpdateStoryCommand
        {
            StoryId = story.PublicId,
            Title = "Updated Title",
            Description = "Updated description",
            ContentType = StoryContentType.Original,
            OriginalSource = "Should be ignored"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        story.OriginalSource.Should().BeNull();
        result.OriginalSource.Should().BeNull();
    }
}
