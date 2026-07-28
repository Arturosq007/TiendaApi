using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaApi.Application.Features.Purchases.Request;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PurchasesController(IPurchaseService _purchaseService) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] PurchaseStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _purchaseService.GetPagedAsync(
            page, pageSize, supplierId, status, from, to);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _purchaseService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseRequest request)
    {
        var userId = GetUserIdFromClaims();
        var result = await _purchaseService.CreateAsync(request, userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPatch("{id:guid}/receive")]
    public async Task<IActionResult> Receive(
        Guid id, [FromBody] ReceivePurchaseRequest request)
    {
        var userId = GetUserIdFromClaims();
        var result = await _purchaseService.ReceiveAsync(id, request, userId);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetUserIdFromClaims();
        await _purchaseService.CancelAsync(id, userId);
        return NoContent();
    }
}