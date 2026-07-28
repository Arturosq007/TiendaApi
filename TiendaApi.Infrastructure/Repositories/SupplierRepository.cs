
using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class SupplierRepository(AppDbContext context) : GenericRepository<Supplier>(context), ISupplierRepository
{
    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.AsQueryable();
        if (excludeId.HasValue)
            query = query.Where(s => s.Id != excludeId.Value);
        return await query.AnyAsync(s => s.Name == name);
    }

    public async Task<PagedResultWithStatus<Supplier>> GetPagedAsync(int page, int pageSize, string? search, bool? isActive)
    {
        var query = _dbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s =>
                s.Name.Contains(search) ||
                s.ContactEmail!.Contains(search) ||
                s.Phone!.Contains(search));
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var queryForCounts = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            queryForCounts = queryForCounts.Where(s =>
                s.Name.Contains(search) ||
                s.ContactEmail!.Contains(search) ||
                s.Phone!.Contains(search));

        var activesCount = await queryForCounts.CountAsync(c => c.IsActive);
        var inactivesCount = await queryForCounts.CountAsync(c => !c.IsActive);

        // ─── Total y paginación ───────────────────────────────────────────────

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultWithStatus<Supplier>(
            items, totalCount, page, pageSize,
            activesCount, inactivesCount);
    }

    public async Task<bool> HasProductsAsync(Guid supplierId)
    {
        return await _context.Products
            .AnyAsync(p => p.SupplierId == supplierId);
    }
}
