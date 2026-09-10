using Be.StoryVerse.Core.Exceptions;
using Content.Application.Commands.Genres.CreateGenre;
using Content.Application.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Commands.Genres;

public class CreateGenreCommandHandlerTests
{
    private readonly IGenreRepository _genreRepository = Substitute.For<IGenreRepository>();
    private readonly ILogger<CreateGenreCommandHandler> _logger =
        Substitute.For<ILogger<CreateGenreCommandHandler>>();

    private readonly CreateGenreCommandHandler _handler;

    public CreateGenreCommandHandlerTests()
    {
        _handler = new CreateGenreCommandHandler(_genreRepository, _logger);
    }

    [Fact]
    public async Task Handle_Should_CreateActiveGenre_When_NameIsUnused()
    {
        _genreRepository.NameExistsAsync("Fantasy", Arg.Any<CancellationToken>()).Returns(false);

        var command = new CreateGenreCommand { Name = "Fantasy", Description = "Fantasy stories", DisplayOrder = 1 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Fantasy");
        result.Slug.Should().Be("fantasy");
        result.IsActive.Should().BeTrue();
        await _genreRepository.Received(1).AddAsync(Arg.Any<Content.Domain.Entities.Genre>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowConflictException_When_NameAlreadyUsed()
    {
        _genreRepository.NameExistsAsync("Fantasy", Arg.Any<CancellationToken>()).Returns(true);

        var command = new CreateGenreCommand { Name = "Fantasy" };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }
}
