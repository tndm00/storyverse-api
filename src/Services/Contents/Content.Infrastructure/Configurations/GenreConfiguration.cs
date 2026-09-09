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

    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.ToTable(InfrastructureConstants.GenresTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

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

        builder.HasData(BuildSeed());
    }

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
