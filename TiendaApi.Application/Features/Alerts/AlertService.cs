using TiendaApi.Application.Features.Alerts.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Alerts;

public class AlertService(IUnitOfWork _uow) : IAlertService
{

    public async Task<IReadOnlyCollection<AlertResponse>> GetUnreadAsync()
    {
        var alerts = await _uow.Alerts.GetUnreadAsync();
        return [.. alerts.Select(MapToDto)];
    }

    public async Task<PagedResult<AlertResponse>> GetPagedAsync(
        int page, int pageSize,
        AlertType? type = null,
        bool? isRead = null)
    {
        var result = await _uow.Alerts.GetPagedAsync(
            page, pageSize, type, isRead);

        var dtos = result.Items.Select(MapToDto).ToList();

        return new PagedResult<AlertResponse>(
            dtos, result.TotalCount,
            result.Page, result.PageSize);
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        var alert = await _uow.Alerts.GetByIdAsync(id)
            ?? throw new NotFoundException("Alerta", id);

        alert.MarkAsRead();

        _uow.Alerts.Update(alert);
        await _uow.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync()
    {
        await _uow.Alerts.MarkAllAsReadAsync();
    }

    // ─── Mapeo ────────────────────────────────────────────────────────────────

    private static AlertResponse MapToDto(Alert alert) => new()
    {
        Id = alert.Id,
        Type = alert.Type.ToString(),
        Message = alert.Message,
        IsRead = alert.IsRead,
        ReferenceId = alert.ReferenceId,
        ReferenceType = alert.ReferenceType,
        CreatedAt = alert.CreatedAt
    };
}