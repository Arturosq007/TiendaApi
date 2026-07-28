using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaApi.Application.Features.CashRegisters.Requests;
using TiendaApi.Application.Interfaces.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CashRegistersController(ICashRegisterService _cashRegisterService) : BaseController
{

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool? isOpen = null)
    {
        var result = await _cashRegisterService.GetPagedAsync(
            page, pageSize, userId, from, to, isOpen);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _cashRegisterService.GetByIdAsync(id);
        return Ok(result);
    }

    // El cajero consulta su turno actual
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent()
    {
        var userId = GetUserIdFromClaims();
        var result = await _cashRegisterService.GetCurrentSessionAsync(userId);

        if (result is null)
            return Ok(new { message = "No tenés un turno abierto actualmente." });

        return Ok(result);
    }

    [HttpPost("open")]
    public async Task<IActionResult> Open([FromBody] OpenCashRegisterRequest request)
    {
        var userId = GetUserIdFromClaims();
        var result = await _cashRegisterService.OpenAsync(request, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPatch("{id:guid}/close")]
    public async Task<IActionResult> Close(
        Guid id, [FromBody] CloseCashRegisterRequest request)
    {
        var userId = GetUserIdFromClaims();
        var result = await _cashRegisterService.CloseAsync(id, request, userId);
        return Ok(result);
    }
}