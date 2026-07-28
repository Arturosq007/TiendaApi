using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class PurchaseRepository(AppDbContext context) : GenericRepository<Purchase>(context), IPurchaseRepository
{
    public async Task<PagedResult<Purchase>> GetPagedAsync(int page, int pageSize, Guid? supplierId,
        PurchaseStatus? status, DateTime? from, DateTime? to)
    {
        var query = _dbSet
            .Include(p => p.Supplier)
            .Include(p => p.User)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(s => s.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(s => s.Date <= to.Value);

        if(supplierId.HasValue)
            query = query.Where(s => s.SupplierId == supplierId.Value);

        if(status.HasValue)
            query = query.Where(s => s.Status == status.Value);


        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Purchase>(items, totalCount, page, pageSize);
    }

    public async Task<Purchase?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Details)
                .ThenInclude(d => d.Product)
            .Include(p => p.Supplier)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
