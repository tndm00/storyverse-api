namespace Content.Domain.Entities;

/// <summary>
/// Join row between a story and a genre. A story may carry several genres but
/// exactly one with <see cref="IsPrimary"/> true — the genre shown prominently
/// and used by discovery/ranking filters.
/// </summary>
public sealed class StoryGenre
{
    public long StoryId { get; set; }

    public long GenreId { get; set; }

    public bool IsPrimary { get; set; }

    public Genre Genre { get; set; }
}
