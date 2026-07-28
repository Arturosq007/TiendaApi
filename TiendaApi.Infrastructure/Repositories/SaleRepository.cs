using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;
public class SaleRepository(AppDbContext context) : GenericRepository<Sale>(context), ISaleRepository
{
    public async Task<Sale?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(s => s.Details)
                .ThenInclude(d => d.Product)
            .Include(s => s.Customer)
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<PagedResult<Sale>> GetPagedAsync(
        int page, int pageSize,
        DateTime? from = null,
        DateTime? to = null,
        Guid? customerId = null,
        Guid? userId = null,
        Guid? cashRegisterId = null,
        bool? isCancelled = null)
    {
        var query = _dbSet
            .Include(s => s.Customer)
            .Include(s => s.User)
            .AsQueryable();

        // ─── Filtros ──────────────────────────────────────────────────────────

        if (from.HasValue)
            query = query.Where(s => s.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(s => s.Date <= to.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (userId.HasValue)
            query = query.Where(s => s.UserId == userId.Value);

        if (cashRegisterId.HasValue)
            query = query.Where(s => s.CashRegisterId == cashRegisterId.Value);

        if (isCancelled.HasValue)
            query = query.Where(s => isCancelled.Value
                ? s.Status == SaleStatus.Cancelada
                : s.Status == SaleStatus.Completada);
        // ─── Total y paginación ───────────────────────────────────────────────

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Sale>(
            items, totalCount, page, pageSize);
    }

    public async Task<decimal> GetTotalByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(s => s.Date >= from &&
                        s.Date <= to &&
                        s.Status != SaleStatus.Cancelada)
            .SumAsync(s => s.Total);
    }
}