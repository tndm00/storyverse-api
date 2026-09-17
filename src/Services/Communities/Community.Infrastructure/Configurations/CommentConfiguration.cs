namespace Community.Infrastructure.Configurations;

/// <summary>
/// Configuration for <see cref="Comment"/>. Indexes support paging comments by
/// chapter/status/time and looking up replies by parent comment.
/// </summary>
public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    /// <summary>Maps <see cref="Comment"/> to its table, keys, indexes and column constraints.</summary>
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        // Table and primary key.
        builder.ToTable(InfrastructureConstants.CommentsTableName, InfrastructureConstants.CommunitySchemaName);

        builder.HasKey(x => x.Id);

        // Lookup indexes: unique public id, chapter comment feed ordering, and reply lookup by parent.
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.ChapterId, x.Status, x.CreatedAt });
        builder.HasIndex(x => x.ParentCommentId);

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.ChapterId).IsRequired();

        builder.Property(x => x.AuthorUserId).IsRequired();

        // Comment content stored as unbounded text.
        builder.Property(x => x.Content)
            .HasColumnType("text")
            .IsRequired();

        // Status persisted as its string name, not the numeric enum value.
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
