using TiendaApi.Application.Features.CashRegisters.Requests;
using TiendaApi.Application.Features.CashRegisters.Response;
using TiendaApi.Domain.Common;

namespace TiendaApi.Application.Interfaces.Services;

public interface ICashRegisterService
{
    Task<CashRegisterResponse> GetByIdAsync(Guid id);
    Task<CashRegisterResponse?> GetCurrentSessionAsync(Guid userId);
    Task<PagedResult<CashRegisterResponse>> GetPagedAsync(
        int page, int pageSize,
        Guid? userId = null,
        DateTime? from = null,
        DateTime? to = null,
        bool? isOpen = null);
    Task<CashRegisterResponse> OpenAsync(OpenCashRegisterRequest request, Guid userId);
    Task<CashRegisterResponse> CloseAsync(Guid id, CloseCashRegisterRequest request, Guid userId);
}