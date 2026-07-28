using TiendaApi.Domain.Common;
using TiendaApi.Domain.Entities;

namespace TiendaApi.Domain.Interfaces.Repositories;
public interface ICustomerRepository : IGenericRepository<Customer>
{
    // muestra los clientes junto con sus ventas pendientes, se puede usar para mostrar en el dashboard o para enviar promociones   
    Task<IReadOnlyCollection<(Customer Customer, IEnumerable<Sale> TopSales)>> GetCustomersWithTopPendingSalesAsync();

    // sirve para obtener los clientes que tienen deuda
    //Task<IReadOnlyCollection<Customer>> GetCustomersWithDebtAsync();
    // sirve para obtener los clientes que han realizado ventas, se puede usar para mostrar en el dashboard o para enviar promociones
    Task<Customer?> GetWithSalesAsync(Guid id);
    Task<PagedResultWithStatus<Customer>> GetPagedAsync(
        int page, int pageSize,
        string? search = null,
        bool? hasDebt = null,
        bool? isActive = null);
}