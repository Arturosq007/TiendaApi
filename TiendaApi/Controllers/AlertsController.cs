using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertsController(IAlertService _alertService) : BaseController
{

    // Dashboard — contador de alertas no leídas
    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var result = await _alertService.GetUnreadAsync();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] AlertType? type = null,
        [FromQuery] bool? isRead = null)
    {
        var result = await _alertService.GetPagedAsync(
            page, pageSize, type, isRead);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _alertService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _alertService.MarkAllAsReadAsync();
        return NoContent();
    }
}