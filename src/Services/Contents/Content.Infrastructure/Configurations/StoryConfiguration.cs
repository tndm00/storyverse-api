namespace Content.Infrastructure.Configurations;

/// <summary>
/// Table/column/index configuration for <see cref="Story"/>. There is no
/// foreign key to AuthorProfile: that row lives in the Authentication service
/// (see the Phase 1A plan, "author identity").
/// </summary>
public sealed class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable(InfrastructureConstants.StoriesTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.AuthorProfileId);
        builder.HasIndex(x => x.Status);

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(ApplicationConstants.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(ApplicationConstants.MaxSlugLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(ApplicationConstants.MaxDescriptionLength);

        builder.Property(x => x.GuestAuthorName)
            .HasMaxLength(100);

        builder.Property(x => x.CoverImageUrl)
            .HasMaxLength(ApplicationConstants.MaxUrlLength);

        builder.Property(x => x.OriginalSource)
            .HasMaxLength(ApplicationConstants.MaxUrlLength);

        builder.Property(x => x.Language)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.AgeRating)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.RatingAvg)
            .HasColumnType("numeric(3,2)");

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Genres)
            .WithOne()
            .HasForeignKey(x => x.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithOne()
            .HasForeignKey(x => x.StoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
