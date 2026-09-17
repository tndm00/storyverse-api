namespace Community.Infrastructure.Configurations;

/// <summary>
/// Configuration for <see cref="Vote"/>. A unique index on
/// <c>(StoryId, UserId, WeekKey)</c> makes casting a vote idempotent within a week.
/// </summary>
public sealed class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    /// <summary>Maps <see cref="Vote"/> to its table, keys, indexes and column constraints.</summary>
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        // Table and primary key.
        builder.ToTable(InfrastructureConstants.VotesTableName, InfrastructureConstants.CommunitySchemaName);

        builder.HasKey(x => x.Id);

        // Unique public id, idempotent one-vote-per-user-per-week enforcement,
        // and a lookup index for tallying votes per story per week.
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
