namespace Notification.Application.Commands.Notifications.CreateNotification;

public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    /// <summary>Validates recipient, type, title/body limits, and the RefType/RefId pairing rule.</summary>
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage(ApplicationErrorConstants.RecipientRequired);

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage(ApplicationErrorConstants.BodyRequired)
            .MaximumLength(ApplicationConstants.MaxBodyLength);

        RuleFor(x => x.RefType)
            .MaximumLength(ApplicationConstants.MaxRefTypeLength);

        // RefType and RefId are a linked pair: either both identify the source object or neither is set.
        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.RefType) == !x.RefId.HasValue)
            .WithMessage(ApplicationErrorConstants.RefTypeAndIdTogether);
    }
}
