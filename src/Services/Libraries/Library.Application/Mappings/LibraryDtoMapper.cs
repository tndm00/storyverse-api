namespace Library.Application.Mappings;

/// <summary>
/// Explicit entity-to-response-DTO projection for the Library service. Entities
/// are never returned directly as API contracts (code-standard.md section 19);
/// this is the single place that translates them.
/// </summary>
public static class LibraryDtoMapper
{
    /// <summary>Maps a <see cref="LibraryEntry"/> entity to its API response DTO.</summary>
    public static LibraryEntryResponseDto ToDto(LibraryEntry entry)
    {
        return new LibraryEntryResponseDto
        {
            Id = entry.PublicId,
            StoryId = entry.StoryId,
            ShelfStatus = entry.ShelfStatus.ToString(),
            AddedAt = entry.AddedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }

    /// <summary>Maps a <see cref="ReadingProgress"/> entity to its API response DTO.</summary>
    public static ReadingProgressResponseDto ToDto(ReadingProgress progress)
    {
        return new ReadingProgressResponseDto
        {
            Id = progress.PublicId,
            StoryId = progress.StoryId,
            LastChapterId = progress.LastChapterId,
            ScrollPercent = progress.ScrollPercent,
            LastReadAt = progress.LastReadAt
        };
    }
}
