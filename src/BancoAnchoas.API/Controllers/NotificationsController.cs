using BancoAnchoas.Application.Common.Models;
using BancoAnchoas.Application.Features.Notifications.Commands.MarkAllAsRead;
using BancoAnchoas.Application.Features.Notifications.Commands.MarkAsRead;
using BancoAnchoas.Application.Features.Notifications.DTOs;
using BancoAnchoas.Application.Features.Notifications.Queries.GetNotifications;
using BancoAnchoas.Application.Features.Notifications.Queries.GetUnreadCount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BancoAnchoas.API.Controllers;

[Authorize]
public class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<NotificationDto>>>> GetAll(
        [FromQuery] bool? isRead, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetNotificationsQuery(isRead, pageNumber, pageSize));
        return Ok(ApiResponse<PaginatedList<NotificationDto>>.Success(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var count = await Mediator.Send(new GetUnreadCountQuery());
        return Ok(ApiResponse<int>.Success(count));
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await Mediator.Send(new MarkAsReadCommand(id));
        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await Mediator.Send(new MarkAllAsReadCommand());
        return NoContent();
    }
}
