namespace Community.Infrastructure.Configurations;

/// <summary>
/// Configuration for <see cref="Rating"/>. A unique index on
/// <c>(StoryId, UserId)</c> enforces one rating per user per story.
/// </summary>
public sealed class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    /// <summary>Maps <see cref="Rating"/> to its table, keys, indexes and column constraints.</summary>
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        // Table and primary key.
        builder.ToTable(InfrastructureConstants.RatingsTableName, InfrastructureConstants.CommunitySchemaName);

        builder.HasKey(x => x.Id);

        // Unique public id, plus one-rating-per-user-per-story enforcement.
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.StoryId, x.UserId }).IsUnique();

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.StoryId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Score).IsRequired();

        // Optional free-text review.
        builder.Property(x => x.ReviewText)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
