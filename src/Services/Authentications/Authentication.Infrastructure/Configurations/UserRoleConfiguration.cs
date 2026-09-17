namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Table/column/key configuration for <see cref="UserRole"/>. Kept out of the
/// entity itself, per code-standard.md section 24 (EF Configuration Rules).
/// </summary>
public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// Applies table, key, index and property mappings for the <see cref="UserRole"/> entity.
    /// </summary>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        // Map to the identity-schema table for user-role grants.
        builder.ToTable(InfrastructureConstants.UserRolesTableName, InfrastructureConstants.IdentitySchemaName);

        // One grant per (user, role); the composite key doubles as the uniqueness
        // guarantee so a role cannot be granted twice.
        builder.HasKey(x => new { x.UserId, x.Role });

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.GrantedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany(user => user.Roles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
