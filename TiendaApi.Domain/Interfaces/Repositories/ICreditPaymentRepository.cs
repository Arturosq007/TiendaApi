using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface ICreditPaymentRepository : IGenericRepository<CreditPayment>
{
    Task<PagedResult<CreditPayment>> GetPagedAsync(
        int page, int pageSize,
        Guid? customerId = null,
        DateTime? from = null,
        DateTime? to = null);
}