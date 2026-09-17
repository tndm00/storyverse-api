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
    /// <summary>
    /// Configures the EF Core mapping for <see cref="ChapterReviewAction"/>: table/schema,
    /// keys, indexes, column constraints and its cascade relationship to <see cref="Chapter"/>.
    /// </summary>
    public void Configure(EntityTypeBuilder<ChapterReviewAction> builder)
    {
        builder.ToTable(
            InfrastructureConstants.ChapterReviewActionsTableName,
            InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        // Public id must be unique; chapter/moderator indexes speed up audit-trail lookups.
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

        // See class summary for why Cascade is required (and safe) here.
        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
