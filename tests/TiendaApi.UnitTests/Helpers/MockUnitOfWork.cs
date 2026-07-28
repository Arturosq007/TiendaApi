using Moq;
using TiendaApi.Domain.Interfaces;
using TiendaApi.Domain.Interfaces.Repositories;

namespace TiendaApi.UnitTests.Helpers;

public class MockUnitOfWork
{
    public Mock<IUnitOfWork> Mock { get; } = new();
    public Mock<IProductRepository> Products { get; } = new();
    public Mock<ISaleRepository> Sales { get; } = new();
    public Mock<ICustomerRepository> Customers { get; } = new();
    public Mock<ICategoryRepository> Categories { get; } = new();
    public Mock<ISupplierRepository> Suppliers { get; } = new();
    public Mock<IStockMovementRepository> StockMovements { get; } = new();
    public Mock<IAlertRepository> Alerts { get; } = new();
    public Mock<ICashRegisterRepository> CashRegisters { get; } = new();
    public Mock<IUserRepository> Users { get; } = new();
    public Mock<IPurchaseRepository> Purchases { get; } = new();
    public Mock<ICreditPaymentRepository> CreditPayments { get; } = new();

    public MockUnitOfWork()
    {
        Mock.Setup(u => u.Products).Returns(Products.Object);
        Mock.Setup(u => u.Sales).Returns(Sales.Object);
        Mock.Setup(u => u.Customers).Returns(Customers.Object);
        Mock.Setup(u => u.Categories).Returns(Categories.Object);
        Mock.Setup(u => u.Suppliers).Returns(Suppliers.Object);
        Mock.Setup(u => u.StockMovements).Returns(StockMovements.Object);
        Mock.Setup(u => u.Alerts).Returns(Alerts.Object);
        Mock.Setup(u => u.CashRegisters).Returns(CashRegisters.Object);
        Mock.Setup(u => u.Users).Returns(Users.Object);
        Mock.Setup(u => u.Purchases).Returns(Purchases.Object);
        Mock.Setup(u => u.CreditPayments).Returns(CreditPayments.Object);
        Mock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
    }

    public IUnitOfWork Object => Mock.Object;
}