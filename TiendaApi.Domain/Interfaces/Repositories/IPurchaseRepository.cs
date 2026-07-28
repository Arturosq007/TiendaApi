using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface IPurchaseRepository : IGenericRepository<Purchase>
{
    Task<Purchase?> GetWithDetailsAsync(Guid id);
    Task<PagedResult<Purchase>> GetPagedAsync(
        int page, int pageSize,
        Guid? supplierId = null,
        PurchaseStatus? status = null,
        DateTime? from = null,
        DateTime? to = null);
}