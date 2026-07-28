using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext context) : GenericRepository<Category>(context), ICategoryRepository
{
    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        return await _dbSet.AnyAsync(c => c.Name.ToLower() == name.ToLower() &&
            (excludeId == null || c.Id != excludeId));
    }

    public async Task<bool> HasProductsAsync(Guid categoryId)
    {
        return await _context.Products
            .AnyAsync(p => p.CategoryId == categoryId);
    }

    public async Task<PagedResultWithStatus<Category>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? isActive = null)  
    {
        // Construimos la query de forma incremental
        // IQueryable no ejecuta nada hasta que llamamos ToListAsync
        var query = _dbSet.AsQueryable();

        // ─── Filtros ──────────────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.Name.Contains(search));

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        // ─── Conteos para PagedResultWithStatus ───────────────────────────────

        // Estos conteos se hacen sobre la query SIN el filtro de isActive
        // para siempre mostrar el total real independiente del filtro aplicado
        var queryForCounts = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            queryForCounts = queryForCounts.Where(c =>
                c.Name.Contains(search));

        var activesCount = await queryForCounts.CountAsync(c => c.IsActive);
        var inactivesCount = await queryForCounts.CountAsync(c => !c.IsActive);

        // ─── Total y paginación ───────────────────────────────────────────────

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultWithStatus<Category>(
            items, totalCount, page, pageSize,
            activesCount, inactivesCount);
    }
}