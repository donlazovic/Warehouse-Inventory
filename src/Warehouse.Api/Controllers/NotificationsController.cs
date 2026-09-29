using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Extensions;
using Warehouse.BusinessLayer.DTOs.Notifications;
using Warehouse.BusinessLayer.Services.Notifications;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<NotificationListDto>> Get([FromQuery] int take = 20, CancellationToken ct = default)
        => Ok(await _service.GetAsync(User.GetUserId(), take, ct));

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        await _service.MarkReadAsync(User.GetUserId(), id, ct);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<object>> MarkAllRead(CancellationToken ct)
        => Ok(new { marked = await _service.MarkAllReadAsync(User.GetUserId(), ct) });
}
