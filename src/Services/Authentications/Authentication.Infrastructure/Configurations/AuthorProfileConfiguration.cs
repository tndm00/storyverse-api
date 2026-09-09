namespace Authentication.Infrastructure.Configurations;

/// <summary>
/// Table/column/index configuration for <see cref="AuthorProfile"/>. Kept out
/// of the entity itself, per code-standard.md section 24 (EF Configuration Rules).
/// </summary>
public sealed class AuthorProfileConfiguration : IEntityTypeConfiguration<AuthorProfile>
{
    public void Configure(EntityTypeBuilder<AuthorProfile> builder)
    {
        builder.ToTable(InfrastructureConstants.AuthorProfilesTableName, InfrastructureConstants.IdentitySchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<AuthorProfile>(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PenName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Bio)
            .HasMaxLength(2000);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.BannerUrl)
            .HasMaxLength(2048);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.PayoutInfo)
            .HasColumnType("jsonb");

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}
