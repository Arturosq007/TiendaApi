using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface IAlertRepository : IGenericRepository<Alert>
{
    Task<IReadOnlyCollection<Alert>> GetUnreadAsync();
    Task<PagedResult<Alert>> GetPagedAsync(
        int page, int pageSize,
        AlertType? type = null,
        bool? isRead = null);
    Task MarkAllAsReadAsync();
}