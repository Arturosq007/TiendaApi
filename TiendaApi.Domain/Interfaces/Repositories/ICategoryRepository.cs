using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
    Task<bool> HasProductsAsync(Guid categoryId);
    Task<PagedResultWithStatus<Category>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? isActive = null);
}