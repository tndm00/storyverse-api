namespace Library.Application.Commands.LibraryEntries.ChangeShelfStatus;

/// <summary>
/// Handles <see cref="ChangeShelfStatusCommand"/> by updating the shelf of an
/// existing library entry belonging to the authenticated caller.
/// </summary>
public sealed class ChangeShelfStatusCommandHandler : ICommandHandler<ChangeShelfStatusCommand, LibraryEntryResponseDto>
{
    private readonly ILibraryEntryRepository _libraryEntryRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<ChangeShelfStatusCommandHandler> _logger;

    public ChangeShelfStatusCommandHandler(
        ILibraryEntryRepository libraryEntryRepository,
        ICurrentUserContext currentUser,
        ILogger<ChangeShelfStatusCommandHandler> logger)
    {
        _libraryEntryRepository = libraryEntryRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Moves the caller's entry for the given story to the requested shelf, if it isn't already there.</summary>
    public async Task<LibraryEntryResponseDto> Handle(ChangeShelfStatusCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        // The entry must already exist for this user/story.
        var entry = await _libraryEntryRepository.GetAsync(userId, request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.LibraryEntryNotFound);

        var shelfStatus = ShelfStatusParser.Parse(request.ShelfStatus);

        // Only touch the database when the shelf actually changes.
        if (entry.ShelfStatus != shelfStatus)
        {
            entry.ShelfStatus = shelfStatus;
            entry.UpdatedAt = DateTime.UtcNow;
            _libraryEntryRepository.Update(entry);
            await _libraryEntryRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                ApplicationLogConstants.LibraryEntryShelfChanged, userId, request.StoryId, shelfStatus);
        }

        return LibraryDtoMapper.ToDto(entry);
    }
}
