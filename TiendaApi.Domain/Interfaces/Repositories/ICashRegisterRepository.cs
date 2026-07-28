using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface ICashRegisterRepository : IGenericRepository<CashRegister>
{
    Task<CashRegister?> GetOpenByUserAsync(Guid userId);
    Task<bool> HasOpenSessionAsync(Guid userId);
    Task<PagedResult<CashRegister>> GetPagedAsync(
        int page, int pageSize,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null,
        bool? isOpen = null);
}