namespace Library.Infrastructure.Configurations;

/// <summary>EF Core mapping for <see cref="LibraryEntry"/>: table, keys, indexes and column constraints.</summary>
public sealed class LibraryEntryConfiguration : IEntityTypeConfiguration<LibraryEntry>
{
    /// <summary>Configures the table mapping, indexes and column constraints for <see cref="LibraryEntry"/>.</summary>
    public void Configure(EntityTypeBuilder<LibraryEntry> builder)
    {
        builder.ToTable(InfrastructureConstants.LibraryEntriesTableName, InfrastructureConstants.LibrarySchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();

        // A reader saves any given story at most once (domain spec unique(user_id, story_id)).
        builder.HasIndex(x => new { x.UserId, x.StoryId }).IsUnique();

        // "My library" always filters by user, most often also by shelf.
        builder.HasIndex(x => new { x.UserId, x.ShelfStatus });

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.StoryId).IsRequired();

        builder.Property(x => x.ShelfStatus)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.AddedAt).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
