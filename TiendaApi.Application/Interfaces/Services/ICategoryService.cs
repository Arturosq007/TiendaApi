using TiendaApi.Application.Features.Categories.Request;
using TiendaApi.Application.Features.Categories.Response;
using TiendaApi.Domain.Common;

namespace TiendaApi.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryResponse> GetByIdAsync(Guid id);
    Task<PagedResultWithStatus<CategoryResponse>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? isActive = null);
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task ToggleActiveAsync(Guid id);
}