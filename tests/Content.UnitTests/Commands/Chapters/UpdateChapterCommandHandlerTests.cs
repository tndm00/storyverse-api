using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Chapters.UpdateChapter;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class UpdateChapterCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IVolumeRepository _volumeRepository = Substitute.For<IVolumeRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<UpdateChapterCommandHandler> _logger =
        Substitute.For<ILogger<UpdateChapterCommandHandler>>();

    private readonly UpdateChapterCommandHandler _handler;

    public UpdateChapterCommandHandlerTests()
    {
        _handler = new UpdateChapterCommandHandler(
            _storyRepository, _chapterRepository, _volumeRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_UpdateChapterFields_When_OwnerEditsDraftChapter()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter
        {
            Id = 1,
            StoryId = 5,
            PublicId = chapterPublicId,
            Title = "Old Title",
            Content = "Old content",
            Status = ChapterStatus.Draft
        };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var command = new UpdateChapterCommand
        {
            ChapterId = chapterPublicId,
            Title = "New Title",
            Content = "New content here",
            OrderIndex = 2m
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        chapter.Title.Should().Be("New Title");
        chapter.Content.Should().Be("New content here");
        chapter.OrderIndex.Should().Be(2m);
        result.Title.Should().Be("New Title");
        _chapterRepository.Received(1).Update(chapter);
    }

    [Fact]
    public async Task Handle_Should_ThrowForbiddenException_When_NonOwnerUpdatesChapter()
    {
        var chapterPublicId = Guid.NewGuid();
        var chapter = new Chapter { Id = 1, StoryId = 5, PublicId = chapterPublicId, Status = ChapterStatus.Draft };
        var story = new Story { Id = 5, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };

        _chapterRepository.GetByPublicIdAsync(chapterPublicId, Arg.Any<CancellationToken>()).Returns(chapter);
        _storyRepository.GetByIdAsync(5, Arg.Any<CancellationToken>()).Returns(story);
        _authorContext.GetAuthorProfileId().Returns(99L);

        var command = new UpdateChapterCommand
        {
            ChapterId = chapterPublicId,
            Title = "New Title",
            Content = "New content"
        };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
