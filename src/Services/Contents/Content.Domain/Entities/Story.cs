namespace Content.Domain.Entities;

/// <summary>
/// A work of fiction. The story is a container; the readable/sellable unit is
/// <see cref="Chapter"/> (product-workflow-context.md section 4). Ownership is on
/// <see cref="AuthorProfileId"/>, never on a user id (rules section 10.1).
/// </summary>
public sealed class Story : BaseEntity
{
    /// <summary>
    /// Owning author profile. Sourced from the caller's <c>author_id</c> JWT claim;
    /// the AuthorProfile row itself lives in the Authentication service, so this is
    /// a plain value with no cross-service foreign key.
    /// </summary>
    public long AuthorProfileId { get; set; }

    /// <summary>
    /// Free-text pen name for a story published anonymously via
    /// <c>POST /v1/stories/guest-publish</c> (<see cref="AuthorProfileId"/> is 0).
    /// Null for stories published by a real author.
    /// </summary>
    public string GuestAuthorName { get; set; }

    /// <summary>Stable public identifier exposed by the API instead of <see cref="BaseEntity.Id"/>.</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    /// <summary>URL-facing unique identifier derived from the title.</summary>
    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; }

    public string CoverImageUrl { get; set; }

    public StoryStatus Status { get; set; } = StoryStatus.Draft;

    public StoryContentType ContentType { get; set; } = StoryContentType.Original;

    /// <summary>Attribution for a translated work; null for originals.</summary>
    public string OriginalSource { get; set; }

    public string Language { get; set; } = "vi";

    public AgeRating AgeRating { get; set; } = AgeRating.General;

    public int ViewCount { get; set; }

    /// <summary>Denormalized follower count. Maintained by the Library service (not Phase 1A).</summary>
    public int FollowCount { get; set; }

    /// <summary>Denormalized rating average. Maintained by the Community service (not Phase 1A).</summary>
    public decimal RatingAvg { get; set; }

    /// <summary>Denormalized rating count. Maintained by the Community service (not Phase 1A).</summary>
    public int RatingCount { get; set; }

    /// <summary>Set once, when the story's first chapter is published.</summary>
    public DateTime? PublishedAt { get; set; }

    public ICollection<StoryGenre> Genres { get; set; } = new List<StoryGenre>();

    public ICollection<StoryTag> Tags { get; set; } = new List<StoryTag>();
}
