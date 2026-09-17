namespace Content.Infrastructure.Configurations;

public sealed class SearchSyncCursorConfiguration : IEntityTypeConfiguration<SearchSyncCursor>
{
    public void Configure(EntityTypeBuilder<SearchSyncCursor> builder)
    {
        builder.ToTable(InfrastructureConstants.SearchSyncCursorTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LastSyncedAt).IsRequired();
    }
}
