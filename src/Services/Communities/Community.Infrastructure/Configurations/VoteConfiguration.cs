namespace Community.Infrastructure.Configurations;

/// <summary>
/// Configuration for <see cref="Vote"/>. A unique index on
/// <c>(StoryId, UserId, WeekKey)</c> makes casting a vote idempotent within a week.
/// </summary>
public sealed class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable(InfrastructureConstants.VotesTableName, InfrastructureConstants.CommunitySchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.StoryId, x.UserId, x.WeekKey }).IsUnique();
        builder.HasIndex(x => new { x.StoryId, x.WeekKey });

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.StoryId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.WeekKey)
            .HasMaxLength(InfrastructureConstants.WeekKeyColumnLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
