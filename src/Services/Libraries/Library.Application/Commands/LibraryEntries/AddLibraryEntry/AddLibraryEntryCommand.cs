namespace Library.Application.Commands.LibraryEntries.AddLibraryEntry;

/// <summary>
/// Adds a story to the caller's personal library (a deliberate follow). The
/// owning user is always the authenticated caller; it is never taken from the
/// request body.
/// </summary>
public sealed class AddLibraryEntryCommand : ICommand<LibraryEntryResponseDto>
{
    public Guid StoryId { get; init; }

    /// <summary>Optional initial shelf; <c>Reading</c> when omitted.</summary>
    public string ShelfStatus { get; init; }
}
