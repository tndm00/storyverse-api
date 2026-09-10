using Content.Application.Commands.Stories.AssignStoryGenres;
using Content.Application.Dtos;
using FluentAssertions;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class AssignStoryGenresCommandValidatorTests
{
    private readonly AssignStoryGenresCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_HaveError_When_MoreThanOneGenreIsPrimary()
    {
        var command = new AssignStoryGenresCommand
        {
            StoryId = Guid.NewGuid(),
            Genres = new List<StoryGenreSelection>
            {
                new("fantasy", true),
                new("romance", true)
            }
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignStoryGenresCommand.Genres));
    }

    [Fact]
    public void Validate_Should_HaveError_When_NoGenreIsPrimary()
    {
        var command = new AssignStoryGenresCommand
        {
            StoryId = Guid.NewGuid(),
            Genres = new List<StoryGenreSelection>
            {
                new("fantasy", false),
                new("romance", false)
            }
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignStoryGenresCommand.Genres));
    }

    [Fact]
    public void Validate_Should_HaveError_When_DuplicateGenreSlugsAreSelected()
    {
        var command = new AssignStoryGenresCommand
        {
            StoryId = Guid.NewGuid(),
            Genres = new List<StoryGenreSelection>
            {
                new("fantasy", true),
                new("fantasy", false)
            }
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AssignStoryGenresCommand.Genres));
    }

    [Fact]
    public void Validate_Should_NotHaveError_When_ExactlyOnePrimaryGenreAndNoDuplicates()
    {
        var command = new AssignStoryGenresCommand
        {
            StoryId = Guid.NewGuid(),
            Genres = new List<StoryGenreSelection>
            {
                new("fantasy", true),
                new("romance", false)
            }
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
