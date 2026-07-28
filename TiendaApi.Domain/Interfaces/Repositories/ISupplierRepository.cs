using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Domain.Interfaces.Repositories;

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<bool> HasProductsAsync(Guid supplierId);
    Task<PagedResultWithStatus<Supplier>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? isActive = null);
}
