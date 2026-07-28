using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;
namespace TiendaApi.Infrastructure.Repositories;

public class CashRegisterRepository(AppDbContext context) : GenericRepository<CashRegister>(context), ICashRegisterRepository
{
    // Por ahora no necesitamos métodos específicos, pero aquí podríamos agregar consultas complejas
    public async Task<CashRegister?> GetOpenByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(cr => cr.User)
            .FirstOrDefaultAsync(cr =>
                cr.UserId == userId &&
                cr.IsOpen);
    }

    public async Task<PagedResult<CashRegister>> GetPagedAsync(int page, int pageSize, Guid? userId = null,
        DateTime? from = null, DateTime? to = null, bool? isOpen = null)
    {
        var query = _dbSet
            .Include(cr => cr.User)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(cr => cr.UserId == userId.Value);
        if (from.HasValue)
            query = query.Where(cr => cr.OpenedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(cr => cr.OpenedAt <= to.Value);
        if (isOpen.HasValue)
            query = query.Where(cr => cr.IsOpen == isOpen.Value);

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<CashRegister>(items, totalItems, page, pageSize);
    }

    public async Task<bool> HasOpenSessionAsync(Guid userId)
    {
        return await _dbSet
            .AnyAsync(cr =>
                cr.UserId == userId &&
                cr.IsOpen);
    }
}