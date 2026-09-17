namespace Moderation.Infrastructure.Configurations;

/// <summary>EF Core mapping for <see cref="Report"/>: table, indexes, and column constraints.</summary>
public sealed class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    /// <summary>Maps <see cref="Report"/> to its table, indexes, and column definitions.</summary>
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        // Table name/schema and primary key.
        builder.ToTable(InfrastructureConstants.ReportsTableName, InfrastructureConstants.ModerationSchemaName);

        builder.HasKey(x => x.Id);

        // Lookup indexes: public id lookup, queue filtering by status/date, by target, by reporter.
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
        builder.HasIndex(x => x.ReporterUserId);

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.ReporterUserId).IsRequired();

        // Enums are persisted as strings for readability in the database.
        builder.Property(x => x.TargetType)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.TargetId).IsRequired();

        builder.Property(x => x.Reason)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnType("text");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
