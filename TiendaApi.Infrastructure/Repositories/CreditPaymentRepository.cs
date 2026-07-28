using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class CreditPaymentRepository(AppDbContext context) : GenericRepository<CreditPayment>(context), ICreditPaymentRepository
{
    public async Task<PagedResult<CreditPayment>> GetPagedAsync(
        int page, int pageSize,
        Guid? customerId = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var query = _dbSet
            .Include(cp => cp.Customer)
            .Include(cp => cp.User)
            .AsQueryable();

        // ─── Filtros ──────────────────────────────────────────────────────────

        if (customerId.HasValue)
            query = query.Where(cp => cp.CustomerId == customerId.Value);

        if (from.HasValue)
            query = query.Where(cp => cp.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(cp => cp.Date <= to.Value);

        // ─── Total y paginación ───────────────────────────────────────────────

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(cp => cp.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CreditPayment>(
            items, totalCount, page, pageSize);
    }
}