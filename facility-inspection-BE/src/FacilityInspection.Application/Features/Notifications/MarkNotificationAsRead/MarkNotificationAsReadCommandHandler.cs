using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using MediatR;

namespace FacilityInspection.Application.Features.Notifications.MarkNotificationAsRead;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand>
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationAsReadCommandHandler(
        INotificationService notificationService,
        ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new ForbiddenAccessException("The current user could not be resolved.");
        }

        await _notificationService.MarkAsReadAsync(request.Id, userId, cancellationToken);
    }
}
