using FluentAssertions;
using Moq;
using TiendaApi.Application.Features.Sales;
using TiendaApi.Application.Features.Sales.Request;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.UnitTests.Helpers;

namespace TiendaApi.UnitTests.Application;

public class SaleServiceTests
{
    private readonly MockUnitOfWork _mockUow;
    private readonly SaleService _saleService;

    public SaleServiceTests()
    {
        _mockUow = new MockUnitOfWork();
        _saleService = new SaleService(_mockUow.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Product CreateActiveProduct(
        int stock = 10,
        decimal salePrice = 5.00m)
    {
        var product = Product.Create(
            name: "Producto Test",
            sku: "TST-001",
            purchasePrice: 2.00m,
            salePrice: salePrice,
            stock: stock,
            minStock: 5,
            categoryId: Guid.NewGuid());

        return product;
    }

    private static Customer CreateActiveCustomer(decimal creditLimit = 500)
    {
        return Customer.Create("Cliente Test", null, null, creditLimit);
    }

    private static CreateSaleRequest CreateRequest(
        SalePaymentType paymentType = SalePaymentType.Efectivo,
        Guid? cashRegisterId = null,
        Guid? customerId = null,
        Guid? productId = null,
        int quantity = 2)
    {
        return new CreateSaleRequest(
            paymentType,
            cashRegisterId,
            customerId,
            [
                new SaleDetailRequest(
                    productId ?? Guid.NewGuid(),
                    quantity
                ),
            ]
        );  
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateSale()
    {
        // Arrange
        var product = CreateActiveProduct(stock: 10);
        var productId = product.Id;
        var userId = Guid.NewGuid();
        var request = CreateRequest(productId: productId, quantity: 2);

        // Configurar GetByIdsAsync para que retorne la lista con nuestro producto real
        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([product]);

        // Configurar el Mock de la Transacción para ejecutar el bloque del servicio
        _mockUow.Mock
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());


        // Configurar GetWithDetailsAsync para evitar retornos nulos que rompan el MapToDto
        _mockUow.Sales
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) =>
            {
                // Creamos una venta simulada idéntica para el mapeo final de respuesta
                var sale = Sale.Create(userId, request.PaymentType, request.CashRegisterId, request.CustomerId);
                return sale;
            });

        // Mocks secundarios de seguridad
        _mockUow.Alerts
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Alert, bool>>>()))
            .ReturnsAsync(false);

        // Act
        var action = async () => await _saleService.CreateAsync(request, userId);

        // Assert
        // No lanza excepción — la venta se creó correctamente
        await action.Should().NotThrowAsync();

        // Verificar que se descontó el stock
        product.Stock.Should().Be(8);

        // Verificar que se llamó a AddAsync
        _mockUow.Sales.Verify(r => r.AddAsync(It.IsAny<Sale>()), Times.Once);
        _mockUow.Mock.Verify(u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateAsync_WithInsufficientStock_ShouldThrow()
    {
        // Arrange
        var product = CreateActiveProduct(stock: 1); // solo 1 en stock
        var productId = product.Id;
        var userId = Guid.NewGuid();

        // CORRECCIÓN: Configurar GetByIdsAsync (en plural)
        _mockUow.Products.Setup(u => u.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([product]);

        // CORRECCIÓN: Configurar Mock de Transacción
        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        // Pedir 5 pero solo hay 1
        var request = CreateRequest(productId: productId, quantity: 5);

        // Act
        var action = async () => await _saleService.CreateAsync(request, userId);

        // Assert
        // Nota: Asegúrate de que la excepción exacta lanzada por product.DeductStock(quantity) 
        // sea InsufficientStockException (o DomainException según tu diseño)
        await action.Should().ThrowAsync<InsufficientStockException>();
    }


    [Fact]
    public async Task CreateAsync_WithNonExistentProduct_ShouldThrow()
    {
        // Arrange
        _mockUow.Products.Setup(u => u.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([]); // Producto vacío / no existe

        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        var request = CreateRequest();

        // Act
        var action = async () => await _saleService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }


    [Fact]
    public async Task CreateAsync_WithFiado_ShouldUpdateCustomerDebt()
    {
        // Arrange
        var product = CreateActiveProduct(stock: 10, salePrice: 5.00m);
        var productId = product.Id;
        var customer = CreateActiveCustomer(creditLimit: 500);
        var customerId = customer.Id;
        var userId = Guid.NewGuid();

        _mockUow.Products.Setup(u => u.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([product]);

        _mockUow.Customers.Setup(u => u.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        var saleSimulada = Sale.Create(userId, SalePaymentType.Fiado, cashRegisterId: Guid.NewGuid(), customerId: customerId);
        saleSimulada.AddDetails([(productId, 2, 5.00m)]);

        _mockUow.Sales.Setup(u => u.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(saleSimulada);

        _mockUow.Alerts.Setup(u => u.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Alert, bool>>>()))
            .ReturnsAsync(false);

        var request = new CreateSaleRequest(
            PaymentType: SalePaymentType.Fiado,
            CashRegisterId: Guid.NewGuid(),
            CustomerId: customerId,
            Details: [new SaleDetailRequest(productId, 2)]
        );

        // Act
        await _saleService.CreateAsync(request, userId);

        // Assert — la deuda del cliente aumentó
        customer.HasDebt().Should().BeTrue();
        customer.CurrentDebt.Should().Be(10);
    }

    [Fact]
    public async Task CreateAsync_WithFiadoAndNoCustomer_ShouldThrow()
    {
        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
           .Returns<Func<Task>>(async action => await action());

        // Arrange
        var request = CreateRequest(paymentType: SalePaymentType.Fiado, customerId: null);
        // customerId es null

        // Act
        var action = async () =>
            await _saleService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<DomainException>()
            .WithMessage("*cliente*obligatorio*");
    }

    [Fact]
    public async Task CreateAsync_WithInactiveCustomer_ShouldThrow()
    {
        // Arrange
        var customer = CreateActiveCustomer();
        var customerId = customer.Id;
        customer.Deactivate(); // cliente inactivo

        var product = CreateActiveProduct(stock: 10);

        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([product]);

        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
           .Returns<Func<Task>>(async action => await action());

        var request = new CreateSaleRequest(
            PaymentType: SalePaymentType.Fiado,
            CashRegisterId: Guid.NewGuid(),
            CustomerId: customerId,
            Details: [new SaleDetailRequest(product.Id, 2)]
        );

        // Act
        var action = async () => await _saleService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<DomainException>()
            .WithMessage("*inactivo*");
    }

    // ─── CancelAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_ShouldRestoreStock()
    {
        // Arrange
        var product = CreateActiveProduct(stock: 8); // stock después de venta
        var productId = product.Id;
        var userId = Guid.NewGuid();

        var sale = Sale.Create(userId, SalePaymentType.Efectivo);
        sale.AddDetails([(productId, 2, 5.00m)]);

        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
          .Returns<Func<Task>>(async action => await action());

        _mockUow.Sales
            .Setup(r => r.GetWithDetailsAsync(sale.Id))
            .ReturnsAsync(sale);

        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([product]);

        // Act
        await _saleService.CancelAsync(sale.Id, userId);

        // Assert — stock restaurado
        product.Stock.Should().Be(10); // 8 + 2 devueltos
        sale.Status.Should().Be(SaleStatus.Cancelada);
    }

    [Fact]
    public async Task CancelAsync_WithFiado_ShouldRevertCustomerDebt()
    {
        // Arrange
        var product = CreateActiveProduct(stock: 8);
        var customer = CreateActiveCustomer();
        var productId = product.Id;
        var customerId = customer.Id;
        var userId = Guid.NewGuid();

        var totalSale = 10.00m;
        customer.AddDebt(totalSale);

        var sale = Sale.Create(userId, SalePaymentType.Fiado, customerId: customerId);
        sale.AddDetails([(productId, 2, 5.00m)]);

        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
          .Returns<Func<Task>>(async action => await action());

        _mockUow.Sales
            .Setup(r => r.GetWithDetailsAsync(sale.Id))
            .ReturnsAsync(sale);

        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([product]);

        _mockUow.Customers
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        await _saleService.CancelAsync(sale.Id, userId);

        // Assert — deuda revertida
        customer.CurrentDebt.Should().Be(0);
        customer.HasDebt().Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ShouldThrow()
    {
        // Arrange
        var sale = Sale.Create(Guid.NewGuid(), SalePaymentType.Efectivo);
        var saleId = sale.Id;
        sale.Cancel();

        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
          .Returns<Func<Task>>(async action => await action());

        _mockUow.Sales
            .Setup(r => r.GetWithDetailsAsync(saleId))
            .ReturnsAsync(sale);

        // Act
        var action = async () =>
            await _saleService.CancelAsync(saleId, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<InvalidEntityStateException>();
    }

    [Fact]
    public async Task CancelAsync_WithNonExistentSale_ShouldThrow()
    {
        // Arrange
        _mockUow.Mock.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
          .Returns<Func<Task>>(async action => await action());

        _mockUow.Sales
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Sale?)null);

        // Act
        var action = async () =>
            await _saleService.CancelAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}