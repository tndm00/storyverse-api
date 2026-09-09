namespace Content.Infrastructure.Configurations;

public sealed class StoryTagConfiguration : IEntityTypeConfiguration<StoryTag>
{
    public void Configure(EntityTypeBuilder<StoryTag> builder)
    {
        builder.ToTable(InfrastructureConstants.StoryTagsTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => new { x.StoryId, x.TagId });

        builder.HasIndex(x => x.TagId);

        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
