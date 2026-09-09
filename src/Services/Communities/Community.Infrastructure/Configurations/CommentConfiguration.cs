namespace Community.Infrastructure.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable(InfrastructureConstants.CommentsTableName, InfrastructureConstants.CommunitySchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.ChapterId, x.Status, x.CreatedAt });
        builder.HasIndex(x => x.ParentCommentId);

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.ChapterId).IsRequired();

        builder.Property(x => x.AuthorUserId).IsRequired();

        builder.Property(x => x.Content)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
