namespace Content.Application.Commands.Genres.CreateGenre;

public sealed class CreateGenreCommandValidator : AbstractValidator<CreateGenreCommand>
{
    public CreateGenreCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxGenreNameLength);

        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
