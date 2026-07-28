using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaApi.Application.Features.Stock.Request;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Enums;


namespace TiendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController(IStockService _stockService) : BaseController
{

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? productId = null,
        [FromQuery] MovementType? type = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await _stockService.GetPagedAsync(
            page, pageSize, productId, type, from, to);

        return Ok(result);
    }

    [HttpGet("movements/product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId)
    {
        var result = await _stockService.GetByProductAsync(productId);
        return Ok(result);
    }

    [HttpPost("adjustments")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManualAdjustment(
        [FromBody] ManualAdjustmentRequest request)
    {
        var userId = GetUserIdFromClaims();
        await _stockService.ManualAdjustmentAsync(request, userId);

        return Ok(new { message = "Ajuste de stock registrado correctamente." });
    }
}