using Be.StoryVerse.Cache.Configurations;
using Be.StoryVerse.Cache.ViewTracking;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace Content.UnitTests.ViewTracking;

public class RedisViewStoreTests
{
    private readonly IViewCountFallback _fallback = Substitute.For<IViewCountFallback>();

    /// <summary>A store with Redis switched off; the connection factory throws to prove no connection is ever opened.</summary>
    private RedisViewStore CreateDisabledStore()
    {
        var redis = new Lazy<IConnectionMultiplexer>(() => throw new InvalidOperationException("Redis must not be touched while disabled."));

        return new RedisViewStore(
            redis,
            Options.Create(new RedisOptions { Enabled = false }),
            _fallback,
            NullLogger<RedisViewStore>.Instance);
    }

    [Fact]
    public async Task RecordStoryViewAsync_Should_UseFallback_When_RedisIsDisabled()
    {
        await CreateDisabledStore().RecordStoryViewAsync(7);

        await _fallback.Received(1).IncrementStoryAsync(7, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordChapterViewAsync_Should_UseFallback_When_RedisIsDisabled()
    {
        await CreateDisabledStore().RecordChapterViewAsync(chapterId: 3, storyId: 7);

        await _fallback.Received(1).IncrementChapterAsync(3, 7, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TakePendingAsync_Should_ReturnEmptyBatch_When_RedisIsDisabled()
    {
        var pending = await CreateDisabledStore().TakePendingAsync();

        pending.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task GetSnapshotAsync_Should_ReportUnavailable_When_RedisIsDisabled()
    {
        var snapshot = await CreateDisabledStore().GetSnapshotAsync(topCount: 5);

        snapshot.IsAvailable.Should().BeFalse();
    }
}
