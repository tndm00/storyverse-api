namespace Notification.Infrastructure.Configurations;

/// <summary>
/// Table/column/index configuration for <see cref="NotificationEntity"/>. There
/// is no foreign key to the recipient User: that row lives in the Authentication
/// service, so <see cref="NotificationEntity.UserId"/> is a plain value.
/// </summary>
public sealed class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    /// <summary>Maps the notification entity to its table, columns, and indexes.</summary>
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable(InfrastructureConstants.NotificationsTableName, InfrastructureConstants.NotificationSchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();

        // Feed query: a user's notifications, newest first, optionally filtered by read state.
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(ApplicationConstants.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasMaxLength(ApplicationConstants.MaxBodyLength)
            .IsRequired();

        builder.Property(x => x.RefType)
            .HasMaxLength(ApplicationConstants.MaxRefTypeLength);

        builder.Property(x => x.IsRead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
