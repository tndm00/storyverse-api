namespace Library.Application.Commands.LibraryEntries.AddLibraryEntry;

/// <summary>
/// Handles <see cref="AddLibraryEntryCommand"/> by creating a new library entry
/// for the authenticated caller, rejecting duplicates for the same story.
/// </summary>
public sealed class AddLibraryEntryCommandHandler : ICommandHandler<AddLibraryEntryCommand, LibraryEntryResponseDto>
{
    private readonly ILibraryEntryRepository _libraryEntryRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<AddLibraryEntryCommandHandler> _logger;

    public AddLibraryEntryCommandHandler(
        ILibraryEntryRepository libraryEntryRepository,
        ICurrentUserContext currentUser,
        ILogger<AddLibraryEntryCommandHandler> logger)
    {
        _libraryEntryRepository = libraryEntryRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Adds the requested story to the caller's library, or throws if it's already there.</summary>
    public async Task<LibraryEntryResponseDto> Handle(AddLibraryEntryCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        // Prevent duplicate entries for the same user/story pair.
        if (await _libraryEntryRepository.ExistsAsync(userId, request.StoryId, cancellationToken))
        {
            throw new ConflictException(ApplicationErrorConstants.LibraryEntryAlreadyExists);
        }

        // Fall back to the default shelf when the caller didn't specify one.
        var shelfStatus = ShelfStatusParser.ParseOrDefault(request.ShelfStatus, ShelfStatus.Reading);
        var now = DateTime.UtcNow;

        var entry = new LibraryEntry
        {
            UserId = userId,
            StoryId = request.StoryId,
            ShelfStatus = shelfStatus,
            AddedAt = now
        };

        // Persist the new library entry.
        await _libraryEntryRepository.AddAsync(entry, cancellationToken);
        await _libraryEntryRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.LibraryEntryAdded, userId, request.StoryId, shelfStatus);

        // Integration point: publish a "StoryFollowed" event so the Content service
        // can increment Story.FollowCount and the Notification service can subscribe
        // this reader to new-chapter alerts. No event bus implementation exists yet
        // (Phase 1); the Library service must never write the Content database directly.

        return LibraryDtoMapper.ToDto(entry);
    }
}
