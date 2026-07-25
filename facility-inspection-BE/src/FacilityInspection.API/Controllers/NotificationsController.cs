using FacilityInspection.Application.Common.Authorization;
using FacilityInspection.Application.Common.Models;
using FacilityInspection.Application.DTOs.Notifications;
using FacilityInspection.Application.Features.Notifications.GetMyNotifications;
using FacilityInspection.Application.Features.Notifications.GetNotifications;
using FacilityInspection.Application.Features.Notifications.MarkNotificationAsRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacilityInspection.API.Controllers;

/// <summary>User notifications.</summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly ISender _mediator;

    public NotificationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lists all notifications (admin), paged.</summary>
    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(PaginatedResult<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<NotificationResponse>>> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetNotificationsQuery { PageNumber = pageNumber, PageSize = pageSize, IsRead = isRead },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists the current user's notifications, paged.</summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PaginatedResult<NotificationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<NotificationResponse>>> GetMyNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetMyNotificationsQuery { PageNumber = pageNumber, PageSize = pageSize, IsRead = isRead },
            cancellationToken);
        return Ok(result);
    }

    /// <summary>Marks one of the current user's notifications as read.</summary>
    [HttpPut("{id:guid}/mark-as-read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationAsReadCommand(id), cancellationToken);
        return NoContent();
    }
}
