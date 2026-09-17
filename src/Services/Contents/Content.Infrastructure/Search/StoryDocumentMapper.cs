namespace Content.Infrastructure.Search;

internal static class StoryDocumentMapper
{
    /// <summary>Chapter text is capped here; see <see cref="StoryDocument.ChapterContent"/>.</summary>
    private const int MaxChapterContentLength = 20_000;

    /// <summary>
    /// Maps a <see cref="Story"/> aggregate and its published chapter text into the flat
    /// <see cref="StoryDocument"/> shape used for Elasticsearch indexing.
    /// </summary>
    public static StoryDocument ToDocument(Story story, string publishedChapterContent)
    {
        // Truncate oversized chapter content so index size/indexing cost stays bounded.
        var content = publishedChapterContent ?? string.Empty;
        if (content.Length > MaxChapterContentLength)
        {
            content = content[..MaxChapterContentLength];
        }

        // Flatten genre/tag collections to their slugs/names for filtering and display.
        return new StoryDocument
        {
            StoryId = story.Id,
            Title = story.Title,
            Description = story.Description ?? string.Empty,
            ChapterContent = content,
            AuthorProfileId = story.AuthorProfileId,
            GuestAuthorName = story.GuestAuthorName,
            GenreSlugs = story.Genres.Select(sg => sg.Genre.Slug).ToArray(),
            GenreNames = story.Genres.Select(sg => sg.Genre.Name).ToArray(),
            TagSlugs = story.Tags.Select(st => st.Tag.Slug).ToArray(),
            Status = story.Status.ToString(),
            Language = story.Language,
            PublishedAt = story.PublishedAt,
            CreatedAt = story.CreatedAt,
            ViewCount = story.ViewCount,
            RatingAvg = story.RatingAvg,
            RatingCount = story.RatingCount
        };
    }
}
