using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Library.Application.Commands.LibraryEntries.AddLibraryEntry;
using Library.Application.Constants;
using Library.Application.Interfaces.Repositories;
using Library.Application.Interfaces.Services;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Library.UnitTests.Commands.LibraryEntries;

public class AddLibraryEntryCommandHandlerTests
{
    private readonly ILibraryEntryRepository _libraryEntryRepository = Substitute.For<ILibraryEntryRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<AddLibraryEntryCommandHandler> _logger =
        Substitute.For<ILogger<AddLibraryEntryCommandHandler>>();

    private readonly AddLibraryEntryCommandHandler _handler;

    public AddLibraryEntryCommandHandlerTests()
    {
        _handler = new AddLibraryEntryCommandHandler(_libraryEntryRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_ThrowConflictException_When_EntryAlreadyExists()
    {
        var storyId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.ExistsAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(true);

        var command = new AddLibraryEntryCommand { StoryId = storyId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage(ApplicationErrorConstants.LibraryEntryAlreadyExists);
        await _libraryEntryRepository.DidNotReceive().AddAsync(Arg.Any<LibraryEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_DefaultToReadingShelf_When_ShelfStatusNotSpecified()
    {
        var storyId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.ExistsAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddLibraryEntryCommand { StoryId = storyId, ShelfStatus = null };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShelfStatus.Should().Be(ShelfStatus.Reading.ToString());
        await _libraryEntryRepository.Received(1).AddAsync(
            Arg.Is<LibraryEntry>(e => e.UserId == 1L && e.StoryId == storyId && e.ShelfStatus == ShelfStatus.Reading),
            Arg.Any<CancellationToken>());
        await _libraryEntryRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_UseRequestedShelf_When_ShelfStatusSpecified()
    {
        var storyId = Guid.NewGuid();
        _currentUser.GetUserId().Returns(1L);
        _libraryEntryRepository.ExistsAsync(1L, storyId, Arg.Any<CancellationToken>()).Returns(false);

        var command = new AddLibraryEntryCommand { StoryId = storyId, ShelfStatus = "PlanToRead" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShelfStatus.Should().Be(ShelfStatus.PlanToRead.ToString());
    }
}
