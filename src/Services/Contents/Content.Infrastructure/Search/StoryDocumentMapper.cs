namespace Content.Infrastructure.Search;

internal static class StoryDocumentMapper
{
    /// <summary>Chapter text is capped here; see <see cref="StoryDocument.ChapterContent"/>.</summary>
    private const int MaxChapterContentLength = 20_000;

    public static StoryDocument ToDocument(Story story, string publishedChapterContent)
    {
        var content = publishedChapterContent ?? string.Empty;
        if (content.Length > MaxChapterContentLength)
        {
            content = content[..MaxChapterContentLength];
        }

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
