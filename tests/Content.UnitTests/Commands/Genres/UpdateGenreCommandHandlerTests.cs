using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Genres.UpdateGenre;
using Content.Application.Interfaces.Repositories;
using Content.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Genres;

public class UpdateGenreCommandHandlerTests
{
    private readonly IGenreRepository _genreRepository = Substitute.For<IGenreRepository>();
    private readonly ILogger<UpdateGenreCommandHandler> _logger =
        Substitute.For<ILogger<UpdateGenreCommandHandler>>();

    private readonly UpdateGenreCommandHandler _handler;

    public UpdateGenreCommandHandlerTests()
    {
        _handler = new UpdateGenreCommandHandler(_genreRepository, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_GenreDoesNotExist()
    {
        _genreRepository.GetBySlugAsync("missing", Arg.Any<CancellationToken>()).Returns((Genre)null);

        var command = new UpdateGenreCommand { Slug = "missing", Name = "New Name" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_Should_ThrowConflictException_When_NewNameAlreadyUsedByAnotherGenre()
    {
        var genre = new Genre { Id = 1, Slug = "fantasy", Name = "Fantasy", IsActive = true };
        _genreRepository.GetBySlugAsync("fantasy", Arg.Any<CancellationToken>()).Returns(genre);
        _genreRepository.NameExistsAsync("Romance", Arg.Any<CancellationToken>()).Returns(true);

        var command = new UpdateGenreCommand { Slug = "fantasy", Name = "Romance" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_Should_UpdateGenre_When_NameIsUnchanged()
    {
        var genre = new Genre { Id = 1, Slug = "fantasy", Name = "Fantasy", IsActive = true, DisplayOrder = 1 };
        _genreRepository.GetBySlugAsync("fantasy", Arg.Any<CancellationToken>()).Returns(genre);

        var command = new UpdateGenreCommand
        {
            Slug = "fantasy",
            Name = "Fantasy",
            Description = "Updated description",
            DisplayOrder = 2,
            IsActive = false
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        genre.Description.Should().Be("Updated description");
        genre.DisplayOrder.Should().Be(2);
        genre.IsActive.Should().BeFalse();
        result.IsActive.Should().BeFalse();
        _genreRepository.Received(1).Update(genre);
    }
}
