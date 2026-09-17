namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Table/column/index configuration for <see cref="RefreshToken"/>. Kept out of
/// the entity itself, per code-standard.md section 24 (EF Configuration Rules).
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Applies table, key, index and property mappings for the <see cref="RefreshToken"/> entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // Map to the identity-schema table and set the primary key.
        builder.ToTable(InfrastructureConstants.RefreshTokensTableName, InfrastructureConstants.IdentitySchemaName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        // Lookups are always by token hash; must be unique so a hash maps to one row.
        builder.HasIndex(x => x.TokenHash).IsUnique();

        // Reuse-detection / logout revoke all of a user's live tokens at once.
        builder.HasIndex(x => new { x.UserId, x.RevokedAt });

        builder.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
