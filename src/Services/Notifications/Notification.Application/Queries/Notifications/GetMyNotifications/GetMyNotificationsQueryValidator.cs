namespace Notification.Application.Queries.Notifications.GetMyNotifications;

public sealed class GetMyNotificationsQueryValidator : AbstractValidator<GetMyNotificationsQuery>
{
    public GetMyNotificationsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage(ApplicationErrorConstants.InvalidPageParameters);

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage(ApplicationErrorConstants.InvalidPageParameters);
    }
}
