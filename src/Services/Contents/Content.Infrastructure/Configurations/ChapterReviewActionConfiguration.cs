namespace Content.Infrastructure.Configurations;

/// <summary>
/// The chapter review audit trail. Rows are append-only — the FK to
/// <see cref="Chapter"/> uses <see cref="DeleteBehavior.Restrict"/> so the
/// history is never cascade-deleted. Mirrors <c>ModerationActionConfiguration</c>.
/// </summary>
public sealed class ChapterReviewActionConfiguration : IEntityTypeConfiguration<ChapterReviewAction>
{
    public void Configure(EntityTypeBuilder<ChapterReviewAction> builder)
    {
        builder.ToTable(
            InfrastructureConstants.ChapterReviewActionsTableName,
            InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.ChapterId);
        builder.HasIndex(x => x.ModeratorUserId);

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.ChapterId).IsRequired();

        builder.Property(x => x.ModeratorUserId).IsRequired();

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasMaxLength(2000);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
