using TiendaApi.Application.Features.Suppliers.Request;
using TiendaApi.Application.Features.Suppliers.Response;
using TiendaApi.Domain.Common;

namespace TiendaApi.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<PagedResultWithStatus<SupplierResponse>> GetPagedAsync(
        int page, int pageSize, string? search, bool? isActive);
    Task<SupplierResponse> GetByIdAsync(Guid id);
    Task<SupplierResponse> CreateAsync(CreateSupplierRequest request);
    Task<SupplierResponse> UpdateAsync(Guid id, UpdateSupplierRequest request);
    Task ToggleActiveAsync(Guid id);
}
