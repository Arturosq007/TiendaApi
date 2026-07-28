using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext context) : GenericRepository<Customer>(context), ICustomerRepository
{

    // Este método obtiene un cliente por su ID, incluyendo sus últimas 10 ventas y pagos de crédito.
    public async Task<Customer?> GetWithSalesAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Sales.OrderByDescending(s => s.Date).Take(10))
            .Include(c => c.CreditPayments.OrderByDescending(cp => cp.Date).Take(10))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<PagedResultWithStatus<Customer>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? hasDebt = null,
        bool? isActive = null)
    {
        var query = _dbSet.AsQueryable();

        // ─── Filtros ──────────────────────────────────────────────────────────

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                c.FullName.Contains(search) ||
                c.Phone!.Contains(search));

        if (hasDebt == true)
            query = query.Where(c => c.CurrentDebt > 0);

        if (hasDebt == false)
            query = query.Where(c => c.CurrentDebt == 0);

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        // ─── Total y paginación ───────────────────────────────────────────────

        var queryForCounts = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            queryForCounts = queryForCounts.Where(c =>
                c.FullName.Contains(search) ||
                c.Phone!.Contains(search));

        if (hasDebt == true)
            queryForCounts = queryForCounts.Where(c => c.CurrentDebt > 0);

        if (isActive.HasValue)
            queryForCounts = queryForCounts.Where(c => c.IsActive == isActive.Value);

        var activesCount = await queryForCounts.CountAsync(c => c.IsActive);
        var inactivesCount = await queryForCounts.CountAsync(c => !c.IsActive);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultWithStatus<Customer>(items, totalCount, activesCount, inactivesCount, page, pageSize);
    }

    // Este método es para obtener clientes con deuda, ordenados por la deuda de mayor a menor.
    //public async Task<IReadOnlyCollection<Customer>> GetWithDebtAsync()
    //{
    //    return await _dbSet
    //        .Where(c => c.CurrentDebt > 0 && c.IsActive)
    //        .OrderByDescending(c => c.CurrentDebt)
    //        .ToListAsync();
    //}

    public async Task<IReadOnlyCollection<(Customer Customer, IEnumerable<Sale> TopSales)>> GetCustomersWithTopPendingSalesAsync()
    {
        var data = await _context.Customers
            .Where(c => c.CurrentDebt > 0 && c.IsActive)
            .Select(c => new
            {
                Customer = c,
                TopSales = _context.Sales
                    .Where(s => s.CustomerId == c.Id && s.Status != SaleStatus.Cancelada && s.PaymentType == SalePaymentType.Fiado)
                    .OrderByDescending(s => s.Date)
                    .Take(5)
                    .ToList()
            })
            .ToListAsync();

        // Convertimos a la tupla requerida por la interfaz
        return data.Select(x => (x.Customer, x.TopSales.AsEnumerable())).ToList();
    }

}