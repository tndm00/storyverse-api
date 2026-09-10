using FluentAssertions;
using Library.Application.Commands.LibraryEntries.AddLibraryEntry;
using Xunit;

namespace Library.UnitTests.Commands.LibraryEntries;

public class AddLibraryEntryCommandValidatorTests
{
    private readonly AddLibraryEntryCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_HaveError_When_StoryIdIsEmpty()
    {
        var command = new AddLibraryEntryCommand { StoryId = Guid.Empty };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddLibraryEntryCommand.StoryId));
    }

    [Fact]
    public void Validate_Should_HaveError_When_ShelfStatusIsNotRecognized()
    {
        var command = new AddLibraryEntryCommand { StoryId = Guid.NewGuid(), ShelfStatus = "NotAShelf" };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddLibraryEntryCommand.ShelfStatus));
    }

    [Fact]
    public void Validate_Should_NotHaveError_When_ShelfStatusIsOmitted()
    {
        var command = new AddLibraryEntryCommand { StoryId = Guid.NewGuid(), ShelfStatus = null };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
