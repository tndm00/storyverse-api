using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Stories.AssignStoryGenres;
using Content.Application.Dtos;
using Content.Application.Interfaces.Repositories;
using Content.Application.Interfaces.Services;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

// Note: exactly-one-primary and duplicate-slug enforcement live in
// AssignStoryGenresCommandValidator, not in this handler (see
// AssignStoryGenresCommandValidatorTests). The handler itself only enforces
// that every requested slug resolves to an active genre.
public class AssignStoryGenresCommandHandlerTests
{
    private readonly IStoryRepository _storyRepository = Substitute.For<IStoryRepository>();
    private readonly IGenreRepository _genreRepository = Substitute.For<IGenreRepository>();
    private readonly ICurrentAuthorContext _authorContext = Substitute.For<ICurrentAuthorContext>();
    private readonly ILogger<AssignStoryGenresCommandHandler> _logger =
        Substitute.For<ILogger<AssignStoryGenresCommandHandler>>();

    private readonly AssignStoryGenresCommandHandler _handler;

    public AssignStoryGenresCommandHandlerTests()
    {
        _handler = new AssignStoryGenresCommandHandler(_storyRepository, _genreRepository, _authorContext, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowBusinessRuleException_When_GenreSlugIsInactiveOrUnknown()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        _genreRepository.GetActiveBySlugsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<Genre>());

        var command = new AssignStoryGenresCommand
        {
            StoryId = story.PublicId,
            Genres = new List<StoryGenreSelection> { new("unknown-genre", true) }
        };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Handle_Should_ReplaceStoryGenres_When_AllSlugsAreActive()
    {
        var story = new Story { Id = 1, PublicId = Guid.NewGuid(), AuthorProfileId = 10 };
        _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, Arg.Any<CancellationToken>())
            .Returns(story);
        _authorContext.GetAuthorProfileId().Returns(10L);

        var genre = new Genre { Id = 1, Slug = "fantasy", Name = "Fantasy", IsActive = true };
        _genreRepository.GetActiveBySlugsAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Genre> { genre });

        var command = new AssignStoryGenresCommand
        {
            StoryId = story.PublicId,
            Genres = new List<StoryGenreSelection> { new("fantasy", true) }
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        story.Genres.Should().ContainSingle(sg => sg.GenreId == genre.Id && sg.IsPrimary);
        result.Genres.Should().ContainSingle(g => g.Slug == "fantasy" && g.IsPrimary);
        _storyRepository.Received(1).Update(story);
    }
}
