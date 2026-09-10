using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Genres.HideGenre;
using Content.Application.Interfaces.Repositories;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Genres;

public class HideGenreCommandHandlerTests
{
    private readonly IGenreRepository _genreRepository = Substitute.For<IGenreRepository>();
    private readonly ILogger<HideGenreCommandHandler> _logger =
        Substitute.For<ILogger<HideGenreCommandHandler>>();

    private readonly HideGenreCommandHandler _handler;

    public HideGenreCommandHandlerTests()
    {
        _handler = new HideGenreCommandHandler(_genreRepository, _logger);
    }

    [Fact]
    public async Task Handle_Should_SetGenreInactive_When_GenreExists()
    {
        var genre = new Genre { Id = 1, Slug = "fantasy", Name = "Fantasy", IsActive = true };
        _genreRepository.GetBySlugAsync("fantasy", Arg.Any<CancellationToken>()).Returns(genre);

        var command = new HideGenreCommand { Slug = "fantasy" };

        var result = await _handler.Handle(command, CancellationToken.None);

        genre.IsActive.Should().BeFalse();
        result.IsActive.Should().BeFalse();
        _genreRepository.Received(1).Update(genre);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_GenreDoesNotExist()
    {
        _genreRepository.GetBySlugAsync("missing", Arg.Any<CancellationToken>()).Returns((Genre)null);

        var command = new HideGenreCommand { Slug = "missing" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
