namespace Content.Application.Dtos;

/// <summary>
/// One genre choice for a story: the genre slug and whether it is the primary
/// (exactly one primary per story). Shared by every command that assigns genres.
/// </summary>
public sealed record StoryGenreSelection(string GenreSlug, bool IsPrimary);
