namespace Library.Application.Dtos;

/// <summary>Payload to add a story to the caller's library.</summary>
public sealed class AddLibraryEntryRequestDto
{
    public Guid StoryId { get; init; }

    /// <summary>Optional initial shelf; defaults to <c>Reading</c> when omitted.</summary>
    public string ShelfStatus { get; init; }
}

/// <summary>Payload to move a library entry to a different shelf.</summary>
public sealed class ChangeShelfStatusRequestDto
{
    public string ShelfStatus { get; init; }
}

/// <summary>One story on the reader's shelf.</summary>
public sealed class LibraryEntryResponseDto
{
    public Guid Id { get; init; }

    public Guid StoryId { get; init; }

    public string ShelfStatus { get; init; }

    public DateTime AddedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
