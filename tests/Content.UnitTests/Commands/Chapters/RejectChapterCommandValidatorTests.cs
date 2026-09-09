using System.Linq;
using Content.Application.Commands.Chapters.RejectChapter;
using FluentAssertions;
using Xunit;

namespace Content.UnitTests.Commands.Chapters;

public class RejectChapterCommandValidatorTests
{
    private readonly RejectChapterCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_HaveError_When_ReasonIsEmpty()
    {
        var command = new RejectChapterCommand { ChapterId = Guid.NewGuid(), Reason = string.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RejectChapterCommand.Reason));
    }

    [Fact]
    public void Validate_Should_HaveError_When_ReasonIsWhitespace()
    {
        var command = new RejectChapterCommand { ChapterId = Guid.NewGuid(), Reason = "   " };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RejectChapterCommand.Reason));
    }

    [Fact]
    public void Validate_Should_HaveError_When_ChapterIdIsEmpty()
    {
        var command = new RejectChapterCommand { ChapterId = Guid.Empty, Reason = "Valid reason" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RejectChapterCommand.ChapterId));
    }

    [Fact]
    public void Validate_Should_NotHaveError_When_ReasonIsProvided()
    {
        var command = new RejectChapterCommand { ChapterId = Guid.NewGuid(), Reason = "Valid reason" };

        var result = _validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(RejectChapterCommand.Reason));
    }
}
