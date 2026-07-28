using TiendaApi.Application.Features.Stock.Request;
using TiendaApi.Application.Features.Stock.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Stock;

public class StockService(IUnitOfWork _uow) : IStockService
{

    public async Task<PagedResult<StockMovementResponse>> GetPagedAsync(
        int page, int pageSize,
        Guid? productId = null,
        MovementType? type = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var result = await _uow.StockMovements.GetPagedAsync(
            page, pageSize, productId, type, from, to);

        var dtos = result.Items.Select(MapToDto).ToList();

        return new PagedResult<StockMovementResponse>(
            dtos, result.TotalCount,
            result.Page, result.PageSize);
    }

    public async Task<IReadOnlyCollection<StockMovementResponse>> GetByProductAsync(
        Guid productId)
    {
        // Verificar que el producto existe
        var productExists = await _uow.Products
            .ExistsAsync(p => p.Id == productId);

        if (!productExists)
            throw new NotFoundException("Producto", productId);

        var result = await _uow.StockMovements.GetPagedAsync(
            page: 1,
            pageSize: 100,
            productId: productId);

        return result.Items.Select(MapToDto).ToList();
    }

    public async Task ManualAdjustmentAsync(
        ManualAdjustmentRequest request, Guid userId)
    {
        var product = await _uow.Products.GetByIdAsync(request.ProductId)
            ?? throw new NotFoundException("Producto", request.ProductId);

        if (!product.IsActive)
            throw new DomainException(
                $"El producto '{product.Name}' no está activo.");

        // Validar que un ajuste negativo no deje stock negativo
        if (!request.IsPositive && product.Stock < request.Quantity)
            throw new DomainException(
                $"No se puede ajustar. Stock actual: {product.Stock}, " +
                $"cantidad a restar: {request.Quantity}.");

        var stockBefore = product.Stock;

        // La entidad actualiza el stock
        if (request.IsPositive)
            product.AddStock(request.Quantity);
        else
            product.DeductStock(request.Quantity);

        // Registrar el movimiento
        var movement = StockMovement.ForManualAdjustment(
            productId: product.Id,
            userId: userId,
            quantity: request.Quantity,
            stockBefore: stockBefore,
            isPositive: request.IsPositive,
            reason: request.Reason);

        _uow.Products.Update(product);
        await _uow.StockMovements.AddAsync(movement);
        await _uow.SaveChangesAsync();
    }

    // ─── Mapeo ────────────────────────────────────────────────────────────────

    private static StockMovementResponse MapToDto(StockMovement movement) => new()
    {
        Id = movement.Id,
        Date = movement.Date,
        Type = movement.Type.ToString(),
        Quantity = movement.Quantity,
        StockBefore = movement.StockBefore,
        StockAfter = movement.StockAfter,
        Reason = movement.Reason,
        ReferenceType = movement.ReferenceType,
        ReferenceId = movement.ReferenceId,
        ProductId = movement.ProductId,
        ProductName = movement.Product?.Name ?? string.Empty,
        ProductSKU = movement.Product?.SKU ?? string.Empty,
        UserId = movement.UserId,
        UserFullName = movement.User?.FullName ?? string.Empty,
        CreatedAt = movement.CreatedAt
    };
}