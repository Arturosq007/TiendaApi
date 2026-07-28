using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaApi.Application.Features.Customers.Request;
using TiendaApi.Application.Interfaces.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController(ICustomerService _customerService) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? hasDebt = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _customerService.GetPagedAsync(
            page, pageSize, search, hasDebt, isActive);

        return Ok(result);
    }

    // Endpoint específico para el dashboard de fiados
    [HttpGet("with-debt")]
    public async Task<IActionResult> GetWithDebt()
    {
        var result = await _customerService.GetWithDebtAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _customerService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request)
    {
        var result = await _customerService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var result = await _customerService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        await _customerService.ToggleActiveAsync(id);
        return NoContent();
    }

    // Registrar un abono a la deuda del cliente
    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> RegisterPayment(
        Guid id,
        [FromBody] RegisterPaymentRequest request)
    {
        var userId = GetUserIdFromClaims();
        await _customerService.RegisterPaymentAsync(id, request, userId);

        return Ok(new { message = "Pago registrado correctamente." });
    }

}