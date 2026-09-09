using Content.Application.Commands.Stories.GuestPublishStory;
using Content.Application.Dtos;
using Content.Application.Interfaces.Persistence;
using Content.Application.Interfaces.Repositories;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class GuestPublishStoryCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IChapterRepository _chapterRepository = Substitute.For<IChapterRepository>();
    private readonly IGenreRepository _genreRepository = Substitute.For<IGenreRepository>();
    private readonly IContentUnitOfWork _unitOfWork = Substitute.For<IContentUnitOfWork>();
    private readonly ILogger<GuestPublishStoryCommandHandler> _logger =
        Substitute.For<ILogger<GuestPublishStoryCommandHandler>>();

    private readonly GuestPublishStoryCommandHandler _handler;

    public GuestPublishStoryCommandHandlerTests()
    {
        _handler = new GuestPublishStoryCommandHandler(
            _storyRepository, _chapterRepository, _genreRepository, _unitOfWork, _logger);

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
    }

    private static GuestPublishStoryCommand CreateCommand(string title = "My Guest Story")
    {
        return new GuestPublishStoryCommand
        {
            GuestPenName = "Anon",
            Title = title,
            Description = "A description",
            Genres = new List<StoryGenreSelection> { new("fantasy", true) },
            ChapterContent = "Once upon a time."
        };
    }

    [Fact]
    public async Task Handle_Should_AppendDashTwoSuffix_When_FirstSlugCollisionOccurs()
    {
        _storyRepository.SlugExistsAsync(Arg.Is<string>(s => s == "my-guest-story"), Arg.Any<CancellationToken>())
            .Returns(true);
        _storyRepository.SlugExistsAsync(Arg.Is<string>(s => s == "my-guest-story-2"), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Slug.Should().Be("my-guest-story-2");
    }

    [Fact]
    public async Task Handle_Should_AppendDashThreeSuffix_When_TwoSlugCollisionsOccur()
    {
        _storyRepository.SlugExistsAsync(Arg.Is<string>(s => s == "my-guest-story"), Arg.Any<CancellationToken>())
            .Returns(true);
        _storyRepository.SlugExistsAsync(Arg.Is<string>(s => s == "my-guest-story-2"), Arg.Any<CancellationToken>())
            .Returns(true);
        _storyRepository.SlugExistsAsync(Arg.Is<string>(s => s == "my-guest-story-3"), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Slug.Should().Be("my-guest-story-3");
    }

    [Fact]
    public async Task Handle_Should_UseBaseSlug_When_NoCollisionExists()
    {
        _storyRepository.SlugExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(CreateCommand(), CancellationToken.None);

        result.Slug.Should().Be("my-guest-story");
    }
}
