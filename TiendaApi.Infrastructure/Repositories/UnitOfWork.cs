using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TiendaApi.Domain.Interfaces;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    // Repositorios — se crean lazy (solo cuando se necesitan)
    private IUserRepository? _users;
    private IProductRepository? _products;
    private ISaleRepository? _sales;
    private IPurchaseRepository? _purchases;
    private ICustomerRepository? _customers;
    private IStockMovementRepository? _stockMovements;
    private ICashRegisterRepository? _cashRegisters;
    private ICategoryRepository? _categories;
    private ISupplierRepository? _suppliers;
    private IAlertRepository? _alerts;
    private ICreditPaymentRepository? _creditPayments;

    // Transacción activa
    private IDbContextTransaction? _transaction;

    // ─── Propiedades lazy ─────────────────────────────────────────────────────

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public ISaleRepository Sales =>
        _sales ??= new SaleRepository(_context);

    public IPurchaseRepository Purchases =>
        _purchases ??= new PurchaseRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public IStockMovementRepository StockMovements =>
        _stockMovements ??= new StockMovementRepository(_context);

    public ICashRegisterRepository CashRegisters =>
        _cashRegisters ??= new CashRegisterRepository(_context);

    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    public ISupplierRepository Suppliers =>
        _suppliers ??= new SupplierRepository(_context);

    public IAlertRepository Alerts =>
        _alerts ??= new AlertRepository(_context);

    public ICreditPaymentRepository CreditPayments =>
        _creditPayments ??= new CreditPaymentRepository(_context);

    // ─── Persistencia ─────────────────────────────────────────────────────────

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // ─── Transacciones ────────────────────────────────────────────────────────

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No hay una transacción activa.");

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null)
            throw new InvalidOperationException("No hay una transacción activa.");

        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    // ─── Dispose ──────────────────────────────────────────────────────────────

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
            await _transaction.DisposeAsync();

        await _context.DisposeAsync();
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}