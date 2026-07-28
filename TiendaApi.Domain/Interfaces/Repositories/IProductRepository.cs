using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null);
    Task<IReadOnlyCollection<Product>> GetLowStockProductsAsync();
    Task<PagedResultWithStatus<Product>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        Guid? categoryId = null,
        Guid? supplierId = null,
        bool? isActive = null,
        bool? lowStock = null);
}