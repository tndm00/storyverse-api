namespace Content.Application.Mappings;

/// <summary>
/// Explicit entity-to-response-DTO projection for the Content service. Entities
/// are never returned directly as API contracts (code-standard.md section 19);
/// this is the single place that translates them.
/// </summary>
public static class ContentDtoMapper
{
    public static StorySummaryResponseDto ToSummary(Story story, string primaryGenre)
    {
        return new StorySummaryResponseDto
        {
            Id = story.PublicId,
            Title = story.Title,
            Slug = story.Slug,
            CoverImageUrl = story.CoverImageUrl,
            Status = story.Status.ToString(),
            AgeRating = story.AgeRating.ToString(),
            PrimaryGenre = primaryGenre,
            ViewCount = story.ViewCount,
            RatingAvg = story.RatingAvg,
            RatingCount = story.RatingCount,
            PublishedAt = story.PublishedAt
        };
    }

    public static StoryDetailResponseDto ToDetail(
        Story story,
        IReadOnlyList<StoryGenreDto> genres,
        IReadOnlyList<string> tags)
    {
        return new StoryDetailResponseDto
        {
            Id = story.PublicId,
            Title = story.Title,
            Slug = story.Slug,
            Description = story.Description,
            CoverImageUrl = story.CoverImageUrl,
            Status = story.Status.ToString(),
            ContentType = story.ContentType.ToString(),
            OriginalSource = story.OriginalSource,
            Language = story.Language,
            AgeRating = story.AgeRating.ToString(),
            AuthorProfileId = story.AuthorProfileId,
            GuestAuthorName = story.GuestAuthorName,
            ViewCount = story.ViewCount,
            FollowCount = story.FollowCount,
            RatingAvg = story.RatingAvg,
            RatingCount = story.RatingCount,
            PublishedAt = story.PublishedAt,
            CreatedAt = story.CreatedAt,
            Genres = genres,
            Tags = tags
        };
    }

    /// <summary>
    /// Builds story detail from an entity whose <see cref="Story.Genres"/> and
    /// <see cref="Story.Tags"/> collections (and their nested Genre/Tag navigations)
    /// have been loaded.
    /// </summary>
    public static StoryDetailResponseDto ToDetail(Story story)
    {
        var genres = story.Genres
            .Where(sg => sg.Genre is not null)
            .OrderByDescending(sg => sg.IsPrimary)
            .ThenBy(sg => sg.Genre.DisplayOrder)
            .Select(sg => new StoryGenreDto
            {
                Name = sg.Genre.Name,
                Slug = sg.Genre.Slug,
                IsPrimary = sg.IsPrimary
            })
            .ToArray();

        var tags = story.Tags
            .Where(st => st.Tag is not null)
            .Select(st => st.Tag.Name)
            .OrderBy(name => name)
            .ToArray();

        return ToDetail(story, genres, tags);
    }

    public static string PrimaryGenreName(Story story)
    {
        return story.Genres.FirstOrDefault(sg => sg.IsPrimary && sg.Genre is not null)?.Genre.Name;
    }

    public static ChapterSummaryResponseDto ToSummary(Chapter chapter, Guid? volumePublicId)
    {
        return new ChapterSummaryResponseDto
        {
            Id = chapter.PublicId,
            VolumeId = volumePublicId,
            Title = chapter.Title,
            OrderIndex = chapter.OrderIndex,
            WordCount = chapter.WordCount,
            Status = chapter.Status.ToString(),
            ViewCount = chapter.ViewCount,
            CommentCount = chapter.CommentCount,
            PublishedAt = chapter.PublishedAt,
            RejectionReason = chapter.RejectionReason
        };
    }

    public static ChapterDetailResponseDto ToDetail(Chapter chapter, Guid storyPublicId, Guid? volumePublicId)
    {
        return new ChapterDetailResponseDto
        {
            Id = chapter.PublicId,
            StoryId = storyPublicId,
            VolumeId = volumePublicId,
            Title = chapter.Title,
            OrderIndex = chapter.OrderIndex,
            Content = chapter.Content,
            WordCount = chapter.WordCount,
            Status = chapter.Status.ToString(),
            ViewCount = chapter.ViewCount,
            CommentCount = chapter.CommentCount,
            ScheduledAt = chapter.ScheduledAt,
            PublishedAt = chapter.PublishedAt,
            CreatedAt = chapter.CreatedAt,
            RejectionReason = chapter.RejectionReason
        };
    }

    public static PendingReviewChapterResponseDto ToPendingReviewDto(Chapter chapter, Story story)
    {
        return new PendingReviewChapterResponseDto
        {
            ChapterId = chapter.PublicId,
            StoryId = story.PublicId,
            StorySlug = story.Slug,
            StoryTitle = story.Title,
            ChapterTitle = chapter.Title,
            WordCount = chapter.WordCount,
            AuthorProfileId = story.AuthorProfileId,
            GuestAuthorName = story.GuestAuthorName,
            Status = chapter.Status.ToString(),
            CreatedAt = chapter.CreatedAt
        };
    }

    public static VolumeResponseDto ToDto(Volume volume)
    {
        return new VolumeResponseDto
        {
            Id = volume.PublicId,
            Title = volume.Title,
            OrderIndex = volume.OrderIndex
        };
    }

    public static GenreResponseDto ToDto(Genre genre)
    {
        return new GenreResponseDto
        {
            Name = genre.Name,
            Slug = genre.Slug,
            Description = genre.Description,
            DisplayOrder = genre.DisplayOrder,
            IsActive = genre.IsActive
        };
    }

    public static TagResponseDto ToDto(Tag tag)
    {
        return new TagResponseDto
        {
            Name = tag.Name,
            Slug = tag.Slug,
            UsageCount = tag.UsageCount
        };
    }
}
