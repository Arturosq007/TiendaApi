using TiendaApi.Application.Features.Sales.Request;
using TiendaApi.Application.Features.Sales.Response;
using TiendaApi.Domain.Common;

namespace TiendaApi.Application.Interfaces.Services;

public interface ISaleService
{
    Task<SaleResponse> GetByIdAsync(Guid id);
    Task<PagedResult<SaleSummaryResponse>> GetPagedAsync(
        int page, int pageSize,
        DateTime? from = null,
        DateTime? to = null,
        Guid? customerId = null,
        Guid? userId = null,
        bool? isCancelled = null);
    Task<SaleResponse> CreateAsync(CreateSaleRequest request, Guid userId);
    Task CancelAsync(Guid id, Guid userId);
    Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to);
}