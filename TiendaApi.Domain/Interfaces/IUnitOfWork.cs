using TiendaApi.Domain.Interfaces.Repositories;

namespace TiendaApi.Domain.Interfaces;
public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ISaleRepository Sales { get; }
    IPurchaseRepository Purchases { get; }
    ICustomerRepository Customers { get; }
    IStockMovementRepository StockMovements { get; }
    ICashRegisterRepository CashRegisters { get; }
    ICategoryRepository Categories { get; }
    ISupplierRepository Suppliers { get; }
    IAlertRepository Alerts { get; }
    ICreditPaymentRepository CreditPayments { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    Task ExecuteTransactionAsync(Func<Task> action);
}