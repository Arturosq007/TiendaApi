using FluentAssertions;
using Moq;
using TiendaApi.Application.Features.Products;
using TiendaApi.Application.Features.Products.Request;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;
using TiendaApi.UnitTests.Helpers;

namespace TiendaApi.UnitTests.Application;

public class ProductServiceTests
{
    private readonly MockUnitOfWork _mockUow;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockUow = new MockUnitOfWork();
        _productService = new ProductService(_mockUow.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Product CreateProduct(
        string name = "Coca Cola",
        decimal purchasePrice = 1.50m,
        decimal salePrice = 2.00m,
        int stock = 10,
        int minStock = 5,
        bool isActive = true)
    {
        var product = Product.Create(
            name: name,
            sku: "BEB-COC-0001",
            purchasePrice: purchasePrice,
            salePrice: salePrice,
            stock: stock,
            minStock: minStock,
            categoryId: Guid.NewGuid());

        if (!isActive) product.Deactivate();

        return product;
    }

    private static Category CreateCategory(string name = "Bebidas")
    {
        return Category.Create(name);
    }

    private static CreateProductRequest CreateRequest(
        string name = "Coca Cola",
        decimal purchasePrice = 1.50m,
        decimal salePrice = 2.00m,
        int initialStock = 10,
        int minStock = 5,
        Guid? categoryId = null)
    {
        return new CreateProductRequest
        {
            Name = name,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            InitialStock = initialStock,
            MinStock = minStock,
            CategoryId = categoryId ?? Guid.NewGuid()
        };
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WithExistingProduct_ShouldReturnDto()
    {
        // Arrange
        var product = CreateProduct();
        var productId = product.Id;

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Coca Cola");
        result.PurchasePrice.Should().Be(1.50m);
        result.SalePrice.Should().Be(2.00m);
        result.Stock.Should().Be(10);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentProduct_ShouldThrow()
    {
        // Arrange
        _mockUow.Products
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        // Act
        var action = async () =>
            await _productService.GetByIdAsync(Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateProduct()
    {
        // Arrange
        var category = CreateCategory();
        var categoryId = category.Id;
        var userId = Guid.NewGuid();
        var request = CreateRequest(categoryId: categoryId);

        _mockUow.Categories
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _mockUow.Categories
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _mockUow.Products
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(0);

        _mockUow.Products
            .Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateProduct());

        // Act
        var result = await _productService.CreateAsync(request, userId);

        // Assert
        _mockUow.Products.Verify(
            r => r.AddAsync(It.IsAny<Product>()), Times.Once);

        _mockUow.Mock.Verify(
            u => u.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateAsync_WithInitialStock_ShouldCreateStockMovement()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateCategory();
        var userId = Guid.NewGuid();
        var request = CreateRequest(
            categoryId: categoryId,
            initialStock: 10);

        _mockUow.Categories
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _mockUow.Categories
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _mockUow.Products
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(0);

        _mockUow.Products
            .Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateProduct());

        // Act
        await _productService.CreateAsync(request, userId);

        // Assert — se creó el movimiento de stock inicial
        _mockUow.StockMovements.Verify(
            r => r.AddAsync(It.IsAny<StockMovement>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithZeroInitialStock_ShouldNotCreateMovement()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateCategory();
        var request = CreateRequest(
            categoryId: categoryId,
            initialStock: 0); // sin stock inicial

        _mockUow.Categories
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _mockUow.Categories
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(category);

        _mockUow.Products
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(0);

        _mockUow.Products
            .Setup(r => r.SkuExistsAsync(It.IsAny<string>(), null))
            .ReturnsAsync(false);

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateProduct(stock: 0));

        // Act
        await _productService.CreateAsync(request, Guid.NewGuid());

        // Assert — no se creó movimiento de stock
        _mockUow.StockMovements.Verify(
            r => r.AddAsync(It.IsAny<StockMovement>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentCategory_ShouldThrow()
    {
        // Arrange
        _mockUow.Categories
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false); // categoría no existe

        var request = CreateRequest();

        // Act
        var action = async () =>
            await _productService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Categoría*");
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentSupplier_ShouldThrow()
    {
        // Arrange
        _mockUow.Categories
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        _mockUow.Suppliers
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Supplier, bool>>>()))
            .ReturnsAsync(false); // proveedor no existe

        var request = CreateRequest();
        request.SupplierId = Guid.NewGuid();

        // Act
        var action = async () =>
            await _productService.CreateAsync(request, Guid.NewGuid());

        // Assert
        await action.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Proveedor*");
    }

    // ─── UpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldUpdateProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateProduct(name: "Coca Cola Original");

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _mockUow.Categories
            .Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        var request = new UpdateProductRequest
        {
            Name = "Coca Cola Zero",
            PurchasePrice = 1.50m,
            SalePrice = 2.50m,
            MinStock = 5,
            CategoryId = Guid.NewGuid()
        };

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(CreateProduct(name: "Coca Cola Zero"));

        // Act
        var result = await _productService.UpdateAsync(productId, request);

        // Assert
        _mockUow.Products.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _mockUow.Mock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentProduct_ShouldThrow()
    {
        // Arrange
        _mockUow.Products
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Product?)null);

        var request = new UpdateProductRequest
        {
            Name = "Test",
            PurchasePrice = 1,
            SalePrice = 2,
            MinStock = 5,
            CategoryId = Guid.NewGuid()
        };

        // Act
        var action = async () =>
            await _productService.UpdateAsync(Guid.NewGuid(), request);

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    // ─── ToggleActiveAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ToggleActiveAsync_WhenActive_ShouldDeactivate()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateProduct(isActive: true);

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        await _productService.ToggleActiveAsync(productId);

        // Assert
        product.IsActive.Should().BeFalse();
        _mockUow.Products.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public async Task ToggleActiveAsync_WhenInactive_ShouldActivate()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateProduct(isActive: false);

        _mockUow.Products
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        await _productService.ToggleActiveAsync(productId);

        // Assert
        product.IsActive.Should().BeTrue();
    }
}