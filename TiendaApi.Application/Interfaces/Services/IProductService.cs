using TiendaApi.Application.Features.Products.Request;
using TiendaApi.Application.Features.Products.Response;
using TiendaApi.Domain.Common;

namespace TiendaApi.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductResponse> GetByIdAsync(Guid id);
    Task<PagedResultWithStatus<ProductResponse>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        Guid? categoryId = null,
        Guid? supplierId = null,
        bool? isActive = null,
        bool? lowStock = null);
    Task<IReadOnlyCollection<ProductResponse>> GetLowStockAsync();
    Task<ProductResponse> CreateAsync(CreateProductRequest request, Guid userId);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request);
    Task ToggleActiveAsync(Guid id);
}