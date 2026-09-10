using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.CreateChapter;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class CreateChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<CreateChapterCommandHandler> _logger =
        Substitute.For<ILogger<CreateChapterCommandHandler>>();

    private readonly CreateChapterCommandHandler _handler;

    public CreateChapterCommandHandlerTests()
    {
        _handler = new CreateChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _authorContext, _logger);
    }

    private Story CreateOwnedStory(long authorProfileId, long storyId = 5)
    {
        var story = new Story { Id = storyId, PublicId = Guid.NewGuid(), AuthorProfileId = authorProfileId };
        _authorContext.GetAuthorProfileId().Returns(authorProfileId);
        _storyRepository.GetByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>()).Returns(story);
        return story;
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessRuleException_When_VolumeBelongsToDifferentStory()
    {
        var story = CreateOwnedStory(authorProfileId: 10);
        var volumePublicId = Guid.NewGuid();
        var volume = new Volume { Id = 1, StoryId = 999, PublicId = volumePublicId };
        _volumeRepository.GetByPublicIdAsync(volumePublicId, Arg.Any<CancellationToken>()).Returns(volume);

        var command = new CreateChapterCommand
        {
            StoryId = story.PublicId,
            Title = "Chapter 1",
            Content = "Some content",
            VolumeId = volumePublicId
        };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessRuleException_When_PublishImmediatelyWithoutExactlyOnePrimaryGenre()
    {
        var story = CreateOwnedStory(authorProfileId: 10);
        _storyRepository.HasExactlyOnePrimaryGenreAsync(story.Id, Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateChapterCommand
        {
            StoryId = story.PublicId,
            Title = "Chapter 1",
            Content = "Some content",
            PublishImmediately = true
        };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
