using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Stories.QuickPublishStory;
using Content.Application.Dtos;
using Content.Application.Interfaces.Persistence;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using Content.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class QuickPublishStoryCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IGenreRepository _genreRepository = Substitute.For<IGenreRepository>();
    private readonly ITagRepository _tagRepository = Substitute.For<ITagRepository>();
    private readonly IContentUnitOfWork _unitOfWork = Substitute.For<IContentUnitOfWork>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<QuickPublishStoryCommandHandler> _logger =
        Substitute.For<ILogger<QuickPublishStoryCommandHandler>>();

    private readonly QuickPublishStoryCommandHandler _handler;

    public QuickPublishStoryCommandHandlerTests()
    {
        _handler = new QuickPublishStoryCommandHandler(
            _storyRepository, _chapterRepository, _genreRepository, _tagRepository, _unitOfWork, _authorContext, _logger);

        _unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Func<CancellationToken, Task>>()(CancellationToken.None));

        _genreRepository.GetActiveBySlugsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                IReadOnlyCollection<string> slugs = ci.Arg<IReadOnlyCollection<string>>();
                return (IReadOnlyList<Genre>)slugs
                    .Select(s => new Genre { Id = 1, Slug = s, Name = s })
                    .ToList();
            });

        _storyRepository.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _authorContext.GetAuthorProfileId().Returns(10L);
    }

    private static QuickPublishStoryCommand CreateCommand(string title = "My Novel", bool completeImmediately = false)
    {
        return new QuickPublishStoryCommand
        {
            Title = title,
            Description = "desc",
            Genres = new List<StoryGenreSelection> { new("fantasy", true) },
            ChapterContent = "Chapter one content.",
            CompleteImmediately = completeImmediately
        };
    }

    [Fact]
    public async Task Handle_Should_ThrowConflictException_When_SameAuthorHasStoryWithSameTrimmedTitle()
    {
        _storyRepository.AuthorHasStoryWithTitleAsync(10, "My Novel", Arg.Any<CancellationToken>()).Returns(true);

        Func<Task> act = () => _handler.Handle(CreateCommand("  My Novel  "), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_Should_Succeed_When_DifferentAuthorUsesSameTitle()
    {
        // AuthorHasStoryWithTitleAsync is scoped to this author id; a different author
        // publishing the same title is a distinct authorProfileId and is never blocked.
        _storyRepository.AuthorHasStoryWithTitleAsync(10, "My Novel", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(CreateCommand("My Novel"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Story.Title.Should().Be("My Novel");
    }

    [Fact]
    public async Task Handle_Should_NotChangeCreatedChapterStatus_When_CompleteImmediatelyIsTrue()
    {
        var result = await _handler.Handle(CreateCommand(completeImmediately: true), CancellationToken.None);

        result.FirstChapter.Status.Should().Be(ChapterStatus.PendingReview.ToString());
    }

    [Fact]
    public async Task Handle_Should_NotChangeCreatedChapterStatus_When_CompleteImmediatelyIsFalse()
    {
        var result = await _handler.Handle(CreateCommand(completeImmediately: false), CancellationToken.None);

        result.FirstChapter.Status.Should().Be(ChapterStatus.PendingReview.ToString());
    }

    [Fact]
    public async Task Handle_Should_DeriveDescriptionFromChapterContent_When_DescriptionBlank()
    {
        var command = new QuickPublishStoryCommand
        {
            Title = "My Novel",
            Description = "",
            Genres = new List<StoryGenreSelection> { new("fantasy", true) },
            ChapterContent = "Chapter one content."
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Story.Description.Should().Be("Chapter one content.");
    }
}
