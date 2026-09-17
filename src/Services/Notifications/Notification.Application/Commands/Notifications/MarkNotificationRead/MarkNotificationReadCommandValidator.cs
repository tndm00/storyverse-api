namespace Notification.Application.Commands.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    /// <summary>Validates that a notification id is provided.</summary>
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.NotificationId).NotEmpty();
    }
}
