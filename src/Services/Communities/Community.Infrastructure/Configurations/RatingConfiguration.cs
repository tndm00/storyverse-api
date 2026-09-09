namespace Community.Infrastructure.Configurations;

/// <summary>
/// Configuration for <see cref="Rating"/>. A unique index on
/// <c>(StoryId, UserId)</c> enforces one rating per user per story.
/// </summary>
public sealed class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable(InfrastructureConstants.RatingsTableName, InfrastructureConstants.CommunitySchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.StoryId, x.UserId }).IsUnique();

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.StoryId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Score).IsRequired();

        builder.Property(x => x.ReviewText)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
