using Content.Application.Commands.Stories.GuestPublishStory;
using Content.Application.Dtos;
using FluentAssertions;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class GuestPublishStoryCommandValidatorTests
{
    private readonly GuestPublishStoryCommandValidator _validator = new();

    private static GuestPublishStoryCommand CreateCommand(IReadOnlyList<StoryGenreSelection> genres)
    {
        return new GuestPublishStoryCommand
        {
            GuestPenName = "Anon",
            Title = "A Story",
            Description = "desc",
            Genres = genres,
            ChapterContent = "content"
        };
    }

    [Fact]
    public void Validate_Should_HaveError_When_NoPrimaryGenreSelected()
    {
        var command = CreateCommand(new List<StoryGenreSelection>
        {
            new("fantasy", false)
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GuestPublishStoryCommand.Genres));
    }

    [Fact]
    public void Validate_Should_HaveError_When_ZeroGenresSelected()
    {
        var command = CreateCommand(new List<StoryGenreSelection>());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GuestPublishStoryCommand.Genres));
    }

    [Fact]
    public void Validate_Should_HaveError_When_TwoPrimaryGenresSelected()
    {
        var command = CreateCommand(new List<StoryGenreSelection>
        {
            new("fantasy", true),
            new("romance", true)
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(GuestPublishStoryCommand.Genres));
    }

    [Fact]
    public void Validate_Should_NotHaveError_When_ExactlyOnePrimaryGenreSelected()
    {
        var command = CreateCommand(new List<StoryGenreSelection>
        {
            new("fantasy", true),
            new("romance", false)
        });

        var result = _validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(GuestPublishStoryCommand.Genres));
    }
}
