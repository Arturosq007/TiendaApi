using FluentAssertions;
using Moq;
using TiendaApi.Application.Features.Purchases;
using TiendaApi.Application.Features.Purchases.Request;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.UnitTests.Helpers;

namespace TiendaApi.UnitTests.Application;

public class PurchaseServiceTests
{
    private readonly MockUnitOfWork _mockUow;
    private readonly PurchaseService _purchaseService;

    public PurchaseServiceTests()
    {
        _mockUow = new MockUnitOfWork();
        _purchaseService = new PurchaseService(_mockUow.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    private static void SetPrivateProperty(object instance, string propertyName, object value)
    {
        var property = instance.GetType().GetProperty(propertyName);
        if (property == null)
            throw new ArgumentException($"La propiedad '{propertyName}' no existe en {instance.GetType().Name}");

        property.SetValue(instance, value);
    }

    private static Supplier CreateActiveSupplier(string name = "Proveedor Test")
    {
        return Supplier.Create(name);
    }

    private static Product CreateActiveProduct(
        int stock = 10,
        decimal purchasePrice = 2.00m,
        decimal salePrice = 5.00m)
    {
        return Product.Create(
            name: "Producto Test",
            sku: "TST-001",
            purchasePrice: purchasePrice,
            salePrice: salePrice,
            stock: stock,
            minStock: 5,
            categoryId: Guid.NewGuid());
    }

    private static CreatePurchaseRequest CreateRequest(
        Guid? supplierId = null,
        Guid? productId = null,
        string invoiceNumber = "",
        int quantity = 5,
        decimal unitCost = 10.00m)
    {
        return new CreatePurchaseRequest
        (
            PaymentType: PurchasePaymentType.Contado,
            SupplierId: supplierId ?? Guid.NewGuid(),
            InvoiceNumber: invoiceNumber,
            Items:
            [
                new PurchaseItemRequest
                (
                    productId ?? Guid.NewGuid(),
                    quantity,
                    unitCost
                )
            ]
        );
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreatePurchase()
    {
        // Arrange
        var product = CreateActiveProduct();
        var supplier = CreateActiveSupplier();
        var supplierId = supplier.Id;
        var productId = product.Id;
        var userId = Guid.NewGuid();

        _mockUow.Suppliers
            .Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync(supplier);

        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>())) // Ajustado a List<Guid> según firma del servicio
            .ReturnsAsync([product]);

        var purchase = Purchase.Create(supplier.Id, userId, PurchasePaymentType.Contado, "F001-123");
        purchase.AddDetails([(product.Id, 5, 2.00m)]);

        var user = User.Create(
            username: "admin@test.com",
            name: "Administrador",
            lastName: "Sistema",
            passwordHash: "hashedpassword",
            role: UserRole.Admin
        );

        // USANDO EL HELPER: Sorteas el private set de forma elegante
        SetPrivateProperty(purchase, nameof(Purchase.Supplier), supplier);
        SetPrivateProperty(purchase, nameof(Purchase.User), user);

        // También debemos asegurar que cada detalle simule su producto cargado para evitar d.Product.Name nulo
        foreach (var detail in purchase.Details)
        {
            SetPrivateProperty(detail, nameof(PurchaseDetail.Product), product);
        }

        _mockUow.Purchases
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(purchase);

        var request = CreateRequest(supplierId, productId);

        // Act
        var result = await _purchaseService.CreateAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(10.00m);

        _mockUow.Purchases.Verify(
            r => r.AddAsync(It.IsAny<Purchase>()), Times.Once);

        _mockUow.Mock.Verify(
            u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateAsync_WithInactiveSupplier_ShouldThrow()
    {
        // Arrange
        var supplier = CreateActiveSupplier();
        var supplierId = supplier.Id;
        supplier.Deactivate();

        _mockUow.Suppliers
            .Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync(supplier);

        var request = CreateRequest(supplierId: supplierId);

        // Act
        var action = async () =>
            await _purchaseService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<DomainException>()
            .WithMessage("*inactivo*");
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentSupplier_ShouldThrow()
    {
        // Arrange
        _mockUow.Suppliers
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Supplier?)null);

        var request = CreateRequest();

        // Act
        var action = async () =>
            await _purchaseService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Proveedor*");
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentProduct_ShouldThrow()
    {
        // Arrange
        var supplier = CreateActiveSupplier();
        var productId = Guid.NewGuid();
        var supplierId = supplier.Id;

        _mockUow.Suppliers
            .Setup(r => r.GetByIdAsync(supplierId))
            .ReturnsAsync(supplier);

        // Retorna lista vacía — el producto no existe
        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([]);

        var request = CreateRequest(supplierId, productId);

        // Act
        var action = async () =>
            await _purchaseService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Producto*");
    }

    // ─── ReceiveAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ReceiveAsync_WithValidPurchase_ShouldIncreasedStock()
    {
        // Arrange
        var product = CreateActiveProduct(stock: 5);
        var productId = product.Id;
        var userId = Guid.NewGuid();
        var supplier = CreateActiveSupplier();

        var purchase = Purchase.Create(supplier.Id, userId, PurchasePaymentType.Contado);
        var purchaseId = purchase.Id;
        purchase.AddDetails([(productId, 10, 2.00m)]);

        var user = User.Create(
            username: "admin@test.com",
            name: "Administrador",
            lastName: "Sistema",
            passwordHash: "hashedpassword",
            role: UserRole.Admin
        );

        SetPrivateProperty(purchase, nameof(Purchase.Supplier), supplier);
        SetPrivateProperty(purchase, nameof(Purchase.User),user);
        foreach (var detail in purchase.Details)
        {
            SetPrivateProperty(detail, nameof(PurchaseDetail.Product), product);
        }

        _mockUow.Mock
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        _mockUow.Purchases
            .Setup(r => r.GetWithDetailsAsync(purchaseId))
            .ReturnsAsync(purchase);

        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>()))
            .ReturnsAsync([product]);

        var request = new ReceivePurchaseRequest { Items = [] };

        // Act
        await _purchaseService.ReceiveAsync(purchaseId, request, userId);

        // Assert — stock aumentó
        product.Stock.Should().Be(15); // 5 + 10
        purchase.Status.Should().Be(PurchaseStatus.Recibida);

        _mockUow.StockMovements.Verify(
            r => r.AddAsync(It.IsAny<StockMovement>()), Times.Once);
    }

    [Fact]
    public async Task ReceiveAsync_ShouldUpdateProductPurchasePrice()
    {
        // Arrange
        var product = CreateActiveProduct(purchasePrice: 2.00m);
        var productId = product.Id;
        var userId = Guid.NewGuid();
        var supplier = CreateActiveSupplier();

        var purchase = Purchase.Create(
            Guid.NewGuid(), Guid.NewGuid(), PurchasePaymentType.Contado);
        var purchaseId = purchase.Id;
        purchase.AddDetails([(productId, 5, 2.00m)]);

        var user = User.Create(
            username: "admin@test.com",
            name: "Administrador",
            lastName: "Sistema",
            passwordHash: "hashedpassword",
            role: UserRole.Admin
        );

        SetPrivateProperty(purchase, nameof(Purchase.Supplier), supplier);
        SetPrivateProperty(purchase, nameof(Purchase.User), user);
        foreach (var detail in purchase.Details)
        {
            SetPrivateProperty(detail, nameof(PurchaseDetail.Product), product);
        }
        _mockUow.Mock
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        _mockUow.Purchases
            .Setup(r => r.GetWithDetailsAsync(purchaseId))
            .ReturnsAsync(purchase);

        _mockUow.Products
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([product]);

        // Costo real diferente al pactado
        var request = new ReceivePurchaseRequest
        {
            Items =
            [
                new ReceivePurchaseItemRequest
                {
                    ProductId = productId,
                    ReceivedQuantity = 5,
                    ActualUnitCost = 2.50m // costo real
                }
            ]
        };

        // Act
        await _purchaseService.ReceiveAsync(purchaseId, request, userId);

        // Assert — precio de costo actualizado
        product.PurchasePrice.Should().Be(2.50m);
    }

    [Fact]
    public async Task ReceiveAsync_WithAlreadyReceivedPurchase_ShouldThrow()
    {
        // Arrange
        var purchase = Purchase.Create(
            Guid.NewGuid(), Guid.NewGuid(), PurchasePaymentType.Contado);
        var purchaseId = purchase.Id;
        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m)]);
        purchase.Receive(); // ya recibida

        _mockUow.Mock
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        _mockUow.Purchases
            .Setup(r => r.GetWithDetailsAsync(purchaseId))
            .ReturnsAsync(purchase);

        var request = new ReceivePurchaseRequest { Items = [] };

        // Act
        var action = async () =>
            await _purchaseService.ReceiveAsync(purchaseId, request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<InvalidEntityStateException>();
    }

    [Fact]
    public async Task ReceiveAsync_WithNonExistentPurchase_ShouldThrow()
    {
        // Arrange
        _mockUow.Mock
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        _mockUow.Purchases
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Purchase?)null);

        var request = new ReceivePurchaseRequest { Items = [] };

        // Act
        var action = async () =>
            await _purchaseService.ReceiveAsync(
                Guid.NewGuid(), request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // ─── CancelAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_WhenPending_ShouldCancel()
    {
        // Arrange
        var purchase = Purchase.Create(
            Guid.NewGuid(), Guid.NewGuid(), PurchasePaymentType.Contado);
        var purchaseId = purchase.Id;

        _mockUow.Purchases
           .Setup(r => r.GetByIdAsync(purchaseId))
           .ReturnsAsync(purchase);

        // Act
        await _purchaseService.CancelAsync(purchaseId, Guid.NewGuid());

        // Assert
        purchase.Status.Should().Be(PurchaseStatus.Cancelada);
        _mockUow.Purchases.Verify(r => r.Update(purchase), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WhenReceived_ShouldThrow()
    {
        // Arrange
        var purchase = Purchase.Create(
            Guid.NewGuid(), Guid.NewGuid(), PurchasePaymentType.Contado);
        var purchaseId = purchase.Id;
        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m)]);
        purchase.Receive();

        _mockUow.Purchases
            .Setup(r => r.GetByIdAsync(purchaseId))
            .ReturnsAsync(purchase);

        // Act
        var action = async () =>
            await _purchaseService.CancelAsync(purchaseId, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<InvalidEntityStateException>();
    }

    [Fact]
    public async Task CancelAsync_WithNonExistentPurchase_ShouldThrow()
    {
        // Arrange

        _mockUow.Purchases
            .Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Purchase?)null);

        // Act
        var action = async () =>
            await _purchaseService.CancelAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}