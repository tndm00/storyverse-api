using Content.Application.Policies;
using Content.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Content.UnitTests.Policies;

public class ChapterStatusPolicyTests
{
    [Theory]
    [InlineData(ChapterStatus.Draft, true)]
    [InlineData(ChapterStatus.Scheduled, false)]
    [InlineData(ChapterStatus.PendingReview, false)]
    [InlineData(ChapterStatus.InReview, false)]
    [InlineData(ChapterStatus.Published, false)]
    [InlineData(ChapterStatus.Rejected, true)]
    [InlineData(ChapterStatus.Removed, false)]
    public void CanSubmitForReview_Should_ReturnExpected_When_GivenFromStatus(ChapterStatus from, bool expected)
    {
        ChapterStatusPolicy.CanSubmitForReview(from).Should().Be(expected);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft, false)]
    [InlineData(ChapterStatus.Scheduled, false)]
    [InlineData(ChapterStatus.PendingReview, true)]
    [InlineData(ChapterStatus.InReview, false)]
    [InlineData(ChapterStatus.Published, false)]
    [InlineData(ChapterStatus.Rejected, false)]
    [InlineData(ChapterStatus.Removed, false)]
    public void CanStartReview_Should_ReturnExpected_When_GivenFromStatus(ChapterStatus from, bool expected)
    {
        ChapterStatusPolicy.CanStartReview(from).Should().Be(expected);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft, false)]
    [InlineData(ChapterStatus.Scheduled, false)]
    [InlineData(ChapterStatus.PendingReview, false)]
    [InlineData(ChapterStatus.InReview, true)]
    [InlineData(ChapterStatus.Published, false)]
    [InlineData(ChapterStatus.Rejected, false)]
    [InlineData(ChapterStatus.Removed, false)]
    public void CanApprove_Should_ReturnExpected_When_GivenFromStatus(ChapterStatus from, bool expected)
    {
        ChapterStatusPolicy.CanApprove(from).Should().Be(expected);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft, false)]
    [InlineData(ChapterStatus.Scheduled, false)]
    [InlineData(ChapterStatus.PendingReview, false)]
    [InlineData(ChapterStatus.InReview, true)]
    [InlineData(ChapterStatus.Published, false)]
    [InlineData(ChapterStatus.Rejected, false)]
    [InlineData(ChapterStatus.Removed, false)]
    public void CanReject_Should_ReturnExpected_When_GivenFromStatus(ChapterStatus from, bool expected)
    {
        ChapterStatusPolicy.CanReject(from).Should().Be(expected);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft, true)]
    [InlineData(ChapterStatus.Scheduled, false)]
    [InlineData(ChapterStatus.PendingReview, false)]
    [InlineData(ChapterStatus.InReview, false)]
    [InlineData(ChapterStatus.Published, false)]
    [InlineData(ChapterStatus.Rejected, false)]
    [InlineData(ChapterStatus.Removed, false)]
    public void CanSchedule_Should_ReturnExpected_When_GivenFromStatus(ChapterStatus from, bool expected)
    {
        ChapterStatusPolicy.CanSchedule(from).Should().Be(expected);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft, false)]
    [InlineData(ChapterStatus.Scheduled, true)]
    [InlineData(ChapterStatus.PendingReview, false)]
    [InlineData(ChapterStatus.InReview, false)]
    [InlineData(ChapterStatus.Published, false)]
    [InlineData(ChapterStatus.Rejected, false)]
    [InlineData(ChapterStatus.Removed, false)]
    public void CanCancelSchedule_Should_ReturnExpected_When_GivenFromStatus(ChapterStatus from, bool expected)
    {
        ChapterStatusPolicy.CanCancelSchedule(from).Should().Be(expected);
    }

    [Theory]
    [InlineData(ChapterStatus.Draft, false)]
    [InlineData(ChapterStatus.Scheduled, false)]
    [InlineData(ChapterStatus.PendingReview, false)]
    [InlineData(ChapterStatus.InReview, false)]
    [InlineData(ChapterStatus.Published, true)]
    [InlineData(ChapterStatus.Rejected, false)]
    [InlineData(ChapterStatus.Removed, false)]
    public void CanRemove_Should_ReturnExpected_When_GivenFromStatus(ChapterStatus from, bool expected)
    {
        ChapterStatusPolicy.CanRemove(from).Should().Be(expected);
    }
}
