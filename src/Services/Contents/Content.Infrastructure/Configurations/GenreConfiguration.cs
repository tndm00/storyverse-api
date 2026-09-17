namespace Content.Infrastructure.Configurations;

/// <summary>
/// Configuration and seed data for the admin-managed <see cref="Genre"/> list.
/// The seed is the reference taxonomy from product-workflow-context.md section 4;
/// Admin can add/edit/hide genres afterwards via the API — the list is never
/// hard-coded in business logic (rules section 7.6).
/// </summary>
public sealed class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    private static readonly string[] SeedNames =
    {
        "Ngôn Tình", "Kiếm Hiệp", "Tiên Hiệp", "Huyền Huyễn", "Đô Thị", "Trinh Thám",
        "Kinh Dị", "Hài Hước", "Hành Động", "Xuyên Không", "Trọng Sinh", "Dị Giới",
        "Khoa Huyễn", "Quân Sự", "Lịch Sử", "Đồng Nhân", "Light Novel"
    };

    /// <summary>
    /// Configures the EF Core mapping for <see cref="Genre"/>: table/schema, keys,
    /// unique indexes, column constraints and the seeded reference taxonomy.
    /// </summary>
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable(InfrastructureConstants.GenresTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        // Name and slug must both be unique for lookup and URL routing.
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Name)
            .HasMaxLength(ApplicationConstants.MaxGenreNameLength)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(ApplicationConstants.MaxSlugLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(ApplicationConstants.MaxDescriptionLength);

        builder.Property(x => x.CreatedAt).IsRequired();

        // Seed the fixed reference taxonomy on migration.
        builder.HasData(BuildSeed());
    }

    /// <summary>
    /// Builds the deterministic seed rows for the genre reference taxonomy, one per entry
    /// in <see cref="SeedNames"/>, with slugs generated from the display name.
    /// </summary>
    private static IEnumerable<Genre> BuildSeed()
    {
        // Fixed timestamp so migrations stay deterministic across regenerations.
        var seededAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < SeedNames.Length; i++)
        {
            yield return new Genre
            {
                Id = i + 1,
                Name = SeedNames[i],
                Slug = SlugGenerator.Generate(SeedNames[i]),
                DisplayOrder = i + 1,
                IsActive = true,
                CreatedAt = seededAt
            };
        }
    }
}
