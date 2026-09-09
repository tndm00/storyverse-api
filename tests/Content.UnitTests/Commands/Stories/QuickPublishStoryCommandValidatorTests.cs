using Content.Application.Commands.Stories.QuickPublishStory;
using Content.Application.Dtos;
using FluentAssertions;
using Xunit;

namespace Content.UnitTests.Commands.Stories;

public class QuickPublishStoryCommandValidatorTests
{
    private readonly QuickPublishStoryCommandValidator _validator = new();

    private static QuickPublishStoryCommand CreateCommand(IReadOnlyList<StoryGenreSelection> genres)
    {
        return new QuickPublishStoryCommand
        {
            Title = "A Story",
            Description = "desc",
            Genres = genres,
            ChapterContent = "content"
        };
    }

    [Fact]
    public void Validate_Should_HaveError_When_ZeroGenresSelected()
    {
        var command = CreateCommand(new List<StoryGenreSelection>());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(QuickPublishStoryCommand.Genres));
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
        result.Errors.Should().Contain(e => e.PropertyName == nameof(QuickPublishStoryCommand.Genres));
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

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(QuickPublishStoryCommand.Genres));
    }
}
