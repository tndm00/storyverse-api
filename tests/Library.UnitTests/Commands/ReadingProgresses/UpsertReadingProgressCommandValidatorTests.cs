using FluentAssertions;
using Library.Application.Commands.ReadingProgresses.UpsertReadingProgress;
using Xunit;

namespace Library.UnitTests.Commands.ReadingProgresses;

public class UpsertReadingProgressCommandValidatorTests
{
    private readonly UpsertReadingProgressCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_HaveError_When_StoryIdIsEmpty()
    {
        var command = new UpsertReadingProgressCommand { StoryId = Guid.Empty, LastChapterId = Guid.NewGuid() };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpsertReadingProgressCommand.StoryId));
    }

    [Fact]
    public void Validate_Should_HaveError_When_LastChapterIdIsEmpty()
    {
        var command = new UpsertReadingProgressCommand { StoryId = Guid.NewGuid(), LastChapterId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpsertReadingProgressCommand.LastChapterId));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_Should_HaveError_When_ScrollPercentIsOutOfRange(decimal scrollPercent)
    {
        var command = new UpsertReadingProgressCommand
        {
            StoryId = Guid.NewGuid(),
            LastChapterId = Guid.NewGuid(),
            ScrollPercent = scrollPercent
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpsertReadingProgressCommand.ScrollPercent));
    }

    [Fact]
    public void Validate_Should_NotHaveError_When_ScrollPercentIsWithinRange()
    {
        var command = new UpsertReadingProgressCommand
        {
            StoryId = Guid.NewGuid(),
            LastChapterId = Guid.NewGuid(),
            ScrollPercent = 50m
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
