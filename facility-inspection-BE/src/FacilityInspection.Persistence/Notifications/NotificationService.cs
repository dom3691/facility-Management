using FacilityInspection.Application.Common.Exceptions;
using FacilityInspection.Application.Common.Interfaces;
using FacilityInspection.Application.Common.Interfaces.Repositories;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Domain.Entities;
using FacilityInspection.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FacilityInspection.Persistence.Notifications;

/// <summary>
/// Persistence-backed implementation of <see cref="INotificationService"/>. Queueing enlists
/// in the caller's unit of work; dispatch and mark-as-read commit their own transactions.
/// </summary>
public class NotificationService : INotificationService
{
    private const int DispatchBatchSize = 50;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IIdentityService identityService,
        IDateTimeProvider dateTime,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _identityService = identityService;
        _dateTime = dateTime;
        _logger = logger;
    }

    public async Task QueueNotificationAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            RecipientUserId = request.RecipientUserId,
            Type = request.Type,
            Status = NotificationStatus.Pending,
            Title = request.Title,
            Message = request.Message,
            RelatedEntityName = request.RelatedEntityName,
            RelatedEntityId = request.RelatedEntityId,
            IsRead = false,
        };

        // Enlist only — the caller's SaveChangesAsync commits it with the business change.
        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task SendPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _unitOfWork.Notifications.GetPendingAsync(DispatchBatchSize, cancellationToken);
        if (pending.Count == 0)
        {
            return;
        }

        foreach (var notification in pending)
        {
            try
            {
                var email = await _identityService.GetUserEmailAsync(notification.RecipientUserId, cancellationToken);
                notification.RecipientEmail = email;

                if (string.IsNullOrWhiteSpace(email))
                {
                    notification.Status = NotificationStatus.Failed;
                    notification.ErrorMessage = "No email address on file for the recipient.";
                    continue;
                }

                await _emailService.SendEmailAsync(email, notification.Title, notification.Message, cancellationToken);

                notification.Status = NotificationStatus.Sent;
                notification.SentDate = _dateTime.UtcNow;
                notification.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to dispatch notification {NotificationId}", notification.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Dispatched {Count} pending notification(s).", pending.Count);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), notificationId);

        if (notification.RecipientUserId != userId)
        {
            throw new ForbiddenAccessException("You can only update your own notifications.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadDate = _dateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
