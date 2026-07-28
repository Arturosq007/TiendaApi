using TiendaApi.Application.Features.Purchases.Request;
using TiendaApi.Application.Features.Purchases.Response;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Application.Interfaces.Services;

public interface IPurchaseService
{
    Task<PurchaseResponse> GetByIdAsync(Guid id);
    Task<PagedResult<PurchaseSummaryResponse>> GetPagedAsync(int page, int pageSize, 
        Guid? supplierId = null,
        PurchaseStatus? status = null, 
        DateTime? from = null, 
        DateTime? to = null);
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, Guid userId);
    Task<PurchaseResponse> ReceiveAsync(Guid id, ReceivePurchaseRequest request, Guid userId);
    Task CancelAsync(Guid purchaseId, Guid userId);
}