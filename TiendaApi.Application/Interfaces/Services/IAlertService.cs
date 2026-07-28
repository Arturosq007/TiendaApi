using TiendaApi.Application.Features.Alerts.Response;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;

namespace TiendaApi.Application.Interfaces.Services;

public interface IAlertService
{
    Task<IReadOnlyCollection<AlertResponse>> GetUnreadAsync();
    Task<PagedResult<AlertResponse>> GetPagedAsync(
        int page, int pageSize,
        AlertType? type = null,
        bool? isRead = null);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync();
}