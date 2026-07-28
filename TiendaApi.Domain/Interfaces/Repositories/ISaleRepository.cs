using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface ISaleRepository : IGenericRepository<Sale>
{
    Task<Sale?> GetWithDetailsAsync(Guid id);
    Task<PagedResult<Sale>> GetPagedAsync(
        int page, int pageSize,
        DateTime? from = null,
        DateTime? to = null,
        Guid? customerId = null,
        Guid? userId = null,
        Guid? cashRegisterId = null,
        bool? isCancelled = null);
    Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to);
}