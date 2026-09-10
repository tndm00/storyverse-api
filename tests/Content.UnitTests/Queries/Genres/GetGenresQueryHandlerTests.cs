using Content.Application.Interfaces.Repositories;
using Content.Application.Queries.Genres.GetGenres;
using Content.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Content.UnitTests.Queries.Genres;

public class GetGenresQueryHandlerTests
{
    private readonly IGenreRepository _genreRepository = Substitute.For<IGenreRepository>();

    private readonly GetGenresQueryHandler _handler;

    public GetGenresQueryHandlerTests()
    {
        _handler = new GetGenresQueryHandler(_genreRepository);
    }

    [Fact]
    public async Task Handle_Should_ReturnOnlyActiveGenres_When_IncludeInactiveIsFalse()
    {
        var activeGenre = new Genre { Id = 1, Slug = "fantasy", Name = "Fantasy", IsActive = true };
        _genreRepository.GetActiveOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Genre> { activeGenre });

        var query = new GetGenresQuery { IncludeInactive = false };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().ContainSingle(g => g.Slug == "fantasy");
        await _genreRepository.DidNotReceive().GetAllOrderedAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnAllGenres_When_IncludeInactiveIsTrue()
    {
        var activeGenre = new Genre { Id = 1, Slug = "fantasy", Name = "Fantasy", IsActive = true };
        var hiddenGenre = new Genre { Id = 2, Slug = "romance", Name = "Romance", IsActive = false };
        _genreRepository.GetAllOrderedAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Genre> { activeGenre, hiddenGenre });

        var query = new GetGenresQuery { IncludeInactive = true };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().HaveCount(2);
        await _genreRepository.DidNotReceive().GetActiveOrderedAsync(Arg.Any<CancellationToken>());
    }
}
