namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Table/column/index configuration for <see cref="User"/>. Kept out of the
/// entity itself, per code-standard.md section 24 (EF Configuration Rules).
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // "identity" matches the business-boundary schema naming convention in
        // code-standard.md section 21 (Database Schema Rules).
        builder.ToTable(InfrastructureConstants.UsersTableName, InfrastructureConstants.IdentitySchemaName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(256);

        builder.Property(x => x.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ExternalProvider)
            .HasMaxLength(20);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(128);

        // Postgres keeps multiple NULLs distinct, so this uniquely pins a linked
        // provider identity to one account without needing a partial-index filter.
        builder.HasIndex(x => x.ExternalId)
            .IsUnique();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
