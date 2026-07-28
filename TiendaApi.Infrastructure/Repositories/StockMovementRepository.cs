using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class StockMovementRepository(AppDbContext context) : GenericRepository<StockMovement>(context), IStockMovementRepository
{
    public async  Task<PagedResult<StockMovement>> GetPagedAsync(int page, int pageSize, Guid? productId = null, 
        MovementType? type = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .AsQueryable();

        if (productId.HasValue)
            query = query.Where(sm => sm.ProductId == productId.Value);
        if (type.HasValue)
            query = query.Where(sm => sm.Type == type.Value);
        if (from.HasValue)
            query = query.Where(sm => sm.Date >= from.Value);
        if (to.HasValue)
            query = query.Where(sm => sm.Date <= to.Value);
            
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<StockMovement>(items, totalCount, page, pageSize);
    }
}
