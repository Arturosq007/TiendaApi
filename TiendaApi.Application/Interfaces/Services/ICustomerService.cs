using TiendaApi.Application.Features.Customers.Request;
using TiendaApi.Application.Features.Customers.Response;
using TiendaApi.Domain.Common;

namespace TiendaApi.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerResponse> GetByIdAsync(Guid id);
    Task<PagedResultWithStatus<CustomerResponse>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? hasDebt = null,
        bool? isActive = null);
    Task<IReadOnlyCollection<CustomerDebtResponse>> GetWithDebtAsync();
    Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);
    Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request);
    Task ToggleActiveAsync(Guid id);
    Task RegisterPaymentAsync(Guid customerId, RegisterPaymentRequest request, Guid userId);
}