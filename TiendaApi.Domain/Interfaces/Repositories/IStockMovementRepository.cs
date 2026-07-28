using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface IStockMovementRepository : IGenericRepository<StockMovement>
{
    Task<PagedResult<StockMovement>> GetPagedAsync(
        int page, int pageSize,
        Guid? productId = null,
        MovementType? type = null,
        DateTime? from = null,
        DateTime? to = null);

}
