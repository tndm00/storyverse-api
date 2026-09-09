namespace Moderation.Infrastructure.Configurations;

/// <summary>
/// The moderation audit trail. Rows are append-only — no cascade delete from
/// <see cref="Report"/>, so the history survives even if a report row is ever
/// removed (product-workflow-context.md section 4).
/// </summary>
public sealed class ModerationActionConfiguration : IEntityTypeConfiguration<ModerationAction>
{
    public void Configure(EntityTypeBuilder<ModerationAction> builder)
    {
        builder.ToTable(
            InfrastructureConstants.ModerationActionsTableName,
            InfrastructureConstants.ModerationSchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => x.ReportId);
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
        builder.HasIndex(x => x.ModeratorUserId);

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.ModeratorUserId).IsRequired();

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

        builder.HasOne<Report>()
            .WithMany()
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
