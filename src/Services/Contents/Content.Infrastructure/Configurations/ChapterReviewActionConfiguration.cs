namespace Content.Infrastructure.Configurations;

/// <summary>
/// The chapter review audit trail. Rows are append-only at the application
/// layer (no update/delete repository member) — but the FK to
/// <see cref="Chapter"/> uses <see cref="DeleteBehavior.Cascade"/> so that a
/// row is removed along with its owning <see cref="Chapter"/> when that
/// Chapter is hard-deleted. A Chapter is only ever hard-deleted via cascade
/// from a Story hard-delete, and a Story can only be hard-deleted while it is
/// still in Draft status (never published/public) — see
/// <c>DeleteStoryCommandHandler</c>. Restrict would instead make that delete
/// fail with a DB error whenever the chapter has review history, so Cascade
/// is required here; it never fires for a published/public chapter.
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
            .OnDelete(DeleteBehavior.Cascade);
    }
}
