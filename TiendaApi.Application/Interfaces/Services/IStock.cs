using TiendaApi.Application.Features.Stock.Request;
using TiendaApi.Application.Features.Stock.Response;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Application.Interfaces.Services;

public interface IStockService
{
    Task<PagedResult<StockMovementResponse>> GetPagedAsync(
        int page, int pageSize,
        Guid? productId = null,
        MovementType? type = null,
        DateTime? from = null,
        DateTime? to = null);

    Task<IReadOnlyCollection<StockMovementResponse>> GetByProductAsync(Guid productId);
    Task ManualAdjustmentAsync(ManualAdjustmentRequest request, Guid userId);
}