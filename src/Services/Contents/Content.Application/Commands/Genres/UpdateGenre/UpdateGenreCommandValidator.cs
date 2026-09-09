namespace Content.Application.Commands.Genres.UpdateGenre;

public sealed class UpdateGenreCommandValidator : AbstractValidator<UpdateGenreCommand>
{
    public UpdateGenreCommandValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxGenreNameLength);

        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
