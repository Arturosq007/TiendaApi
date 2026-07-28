using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class AlertRepository(AppDbContext context) : GenericRepository<Alert>(context), IAlertRepository
{
    public async Task<IReadOnlyCollection<Alert>> GetUnreadAsync()
    {
        return await _dbSet
            .Where(a => !a.IsRead)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAllAsReadAsync()
    {
        await _dbSet
            .Where(a => !a.IsRead)
            .ExecuteUpdateAsync(a =>
                a.SetProperty(x => x.IsRead, true));
    }

    public async Task<PagedResult<Alert>> GetPagedAsync(
        int page, int pageSize,
        AlertType? type = null,
        bool? isRead = null)
    {
        var query = _dbSet.AsQueryable();

        // ─── Filtros ──────────────────────────────────────────────────────────

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        if (isRead.HasValue)
            query = query.Where(a => a.IsRead == isRead.Value);

        // ─── Total y paginación ───────────────────────────────────────────────

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Alert>(
            items, totalCount, page, pageSize);
    }
}