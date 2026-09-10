using FluentAssertions;
using Library.Application.Dtos;
using Library.Application.Interfaces.Repositories;
using Library.Application.Interfaces.Services;
using Library.Application.Queries.LibraryEntries.GetMyLibrary;
using Library.Domain.Entities;
using Library.Domain.Enums;
using NSubstitute;
using Xunit;

namespace Library.UnitTests.Queries.LibraryEntries;

public class GetMyLibraryQueryHandlerTests
{
    private readonly ILibraryEntryRepository _libraryEntryRepository = Substitute.For<ILibraryEntryRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();

    private readonly GetMyLibraryQueryHandler _handler;

    public GetMyLibraryQueryHandlerTests()
    {
        _handler = new GetMyLibraryQueryHandler(_libraryEntryRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Should_PassParsedShelfFilter_When_ShelfStatusProvided()
    {
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetPagedAsync(1L, ShelfStatus.Completed, 1, 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<LibraryEntry>(), 0));

        var query = new GetMyLibraryQuery { ShelfStatus = "Completed" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        await _libraryEntryRepository.Received(1).GetPagedAsync(
            1L, ShelfStatus.Completed, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PassNullShelfFilter_When_ShelfStatusNotProvided()
    {
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetPagedAsync(1L, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((Array.Empty<LibraryEntry>(), 0));

        var query = new GetMyLibraryQuery { ShelfStatus = null };

        await _handler.Handle(query, CancellationToken.None);

        await _libraryEntryRepository.Received(1).GetPagedAsync(
            1L, (ShelfStatus?)null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_MapEntriesAndTotalCount_Into_PagedResponse()
    {
        var storyId = Guid.NewGuid();
        var entry = new LibraryEntry { Id = 1, StoryId = storyId, ShelfStatus = ShelfStatus.Reading };
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.GetPagedAsync(1L, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new[] { entry }, 1));

        var query = new GetMyLibraryQuery();

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.Single().StoryId.Should().Be(storyId);
        result.TotalCount.Should().Be(1);
    }
}
