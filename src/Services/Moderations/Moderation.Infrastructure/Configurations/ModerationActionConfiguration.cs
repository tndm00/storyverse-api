namespace Moderation.Infrastructure.Configurations;

/// <summary>
/// The moderation audit trail. Rows are append-only — no cascade delete from
/// <see cref="Report"/>, so the history survives even if a report row is ever
/// removed (product-workflow-context.md section 4).
/// </summary>
public sealed class ModerationActionConfiguration : IEntityTypeConfiguration<ModerationAction>
{
    /// <summary>Maps <see cref="ModerationAction"/> to its table, indexes, columns, and the restrict-delete FK to <see cref="Report"/>.</summary>
    public void Configure(EntityTypeBuilder<ModerationAction> builder)
    {
        // Table name/schema and primary key.
        builder.ToTable(
            InfrastructureConstants.ModerationActionsTableName,
            InfrastructureConstants.ModerationSchemaName);

        builder.HasKey(x => x.Id);

        // Lookup indexes: public id lookup, by report, by target, by moderator.
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.ReportId);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
        builder.HasIndex(x => x.ModeratorUserId);

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.ModeratorUserId).IsRequired();

        // Enums are persisted as strings for readability in the database.
        builder.Property(x => x.TargetType)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.TargetId).IsRequired();

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.Note)
            .HasColumnType("text");

        builder.Property(x => x.CreatedAt).IsRequired();

        // Restrict delete: the audit trail must not be removed when a report is deleted.
        builder.HasOne<Report>()
            .WithMany()
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
