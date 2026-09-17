namespace Content.Infrastructure.Configurations;

/// <summary>
/// Configuration for the <see cref="SearchSyncCursor"/> entity, which tracks the last time
/// the search index was synchronized with the content database.
/// </summary>
public sealed class SearchSyncCursorConfiguration : IEntityTypeConfiguration<SearchSyncCursor>
{
    /// <summary>
    /// Configures the EF Core mapping for <see cref="SearchSyncCursor"/>: table/schema, key
    /// and the required last-synced timestamp column.
    /// </summary>
    public void Configure(EntityTypeBuilder<SearchSyncCursor> builder)
    {
        builder.ToTable(InfrastructureConstants.SearchSyncCursorTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LastSyncedAt).IsRequired();
    }
}
