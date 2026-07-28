using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class ProductRepository(AppDbContext context) : GenericRepository<Product>(context), IProductRepository
{
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null)
    {
        return await _dbSet.AnyAsync(p =>
            p.SKU == sku &&
            (excludeId == null || p.Id != excludeId));
    }

    public async Task<IReadOnlyCollection<Product>> GetLowStockProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive && p.Stock <= p.MinStock)
            .Include(p => p.Category)
            .OrderBy(p => p.Stock)
            .ToListAsync();
    }

    public async Task<PagedResultWithStatus<Product>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        Guid? categoryId = null,
        Guid? supplierId = null,
        bool? isActive = null,
        bool? lowStock = null)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        // ─── Filtros ──────────────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.SKU.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (lowStock == true)
            query = query.Where(p => p.Stock <= p.MinStock);

        // ─── Conteos ──────────────────────────────────────────────────────────

        var queryForCounts = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            queryForCounts = queryForCounts.Where(p =>
                p.Name.Contains(search) ||
                p.SKU.Contains(search));

        if (categoryId.HasValue)
            queryForCounts = queryForCounts
                .Where(p => p.CategoryId == categoryId.Value);

        if (supplierId.HasValue)
            queryForCounts = queryForCounts
                .Where(p => p.SupplierId == supplierId.Value);

        var activesCount = await queryForCounts.CountAsync(p => p.IsActive);
        var inactivesCount = await queryForCounts.CountAsync(p => !p.IsActive);

        // ─── Total y paginación ───────────────────────────────────────────────

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultWithStatus<Product>(
            items, totalCount, page, pageSize,
            activesCount, inactivesCount);
    }

    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        return await _dbSet
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();
    }
}