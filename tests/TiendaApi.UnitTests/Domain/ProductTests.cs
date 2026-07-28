using FluentAssertions;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.UnitTests.Domain;

public class ProductTests
{
    // ─── Factory ─────────────────────────────────────────────────────────────

    private static Product CreateProduct(
        string name = "Coca Cola",
        decimal purchasePrice = 1.50m,
        decimal salePrice = 2.00m,
        int initialStock = 10,
        int minStock = 5)
    {
        return Product.Create(
            name: name,
            sku: "BEB-COC-0001",
            purchasePrice: purchasePrice,
            salePrice: salePrice,
            minStock: minStock,
            stock: initialStock,
            categoryId: Guid.NewGuid());
    }

    // ─── DeductStock ──────────────────────────────────────────────────────────

    [Fact]
    public void DeductStock_WithValidQuantity_ShouldReduceStock()
    {
        var product = CreateProduct(initialStock: 10);

        product.DeductStock(3);

        product.Stock.Should().Be(7);
    }

    [Fact]
    public void DeductStock_WithExactStock_ShouldReduceToZero()
    {
        var product = CreateProduct(initialStock: 5);

        product.DeductStock(5);

        product.Stock.Should().Be(0);
    }

    [Fact]
    public void DeductStock_WithInsufficientStock_ShouldThrow()
    {
        var product = CreateProduct(initialStock: 3);

        var action = () => product.DeductStock(5);

        action.Should().Throw<InsufficientStockException>()
            .WithMessage("*Coca Cola*");
    }

    [Fact]
    public void DeductStock_WithZeroQuantity_ShouldThrow()
    {
        var product = CreateProduct(initialStock: 10);

        var action = () => product.DeductStock(0);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void DeductStock_WithNegativeQuantity_ShouldThrow()
    {
        var product = CreateProduct(initialStock: 10);

        var action = () => product.DeductStock(-1);

        action.Should().Throw<DomainException>();
    }

    // ─── AddStock ────────────────────────────────────────────────────────────

    [Fact]
    public void AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        var product = CreateProduct(initialStock: 10);

        product.AddStock(5);

        product.Stock.Should().Be(15);
    }

    [Fact]
    public void AddStock_WithZeroQuantity_ShouldThrow()
    {
        var product = CreateProduct(initialStock: 10);

        var action = () => product.AddStock(0);

        action.Should().Throw<DomainException>();
    }

    // ─── IsLowStock / IsOutOfStock ───────────────────────────────────────────

    [Fact]
    public void IsLowStock_WhenStockAtMinimum_ShouldReturnTrue()
    {
        var product = CreateProduct(initialStock: 5, minStock: 5);

        product.IsLowStock().Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WhenStockAboveMinimum_ShouldReturnFalse()
    {
        var product = CreateProduct(initialStock: 10, minStock: 5);

        product.IsLowStock().Should().BeFalse();
    }

    [Fact]
    public void IsOutOfStock_WhenStockIsZero_ShouldReturnTrue()
    {
        var product = CreateProduct(initialStock: 0);

        product.IsOutOfStock().Should().BeTrue();
    }

    // ─── Margin ──────────────────────────────────────────────────────────────

    [Fact]
    public void GetMarginPercent_ShouldCalculateCorrectly()
    {
        // PurchasePrice = 1.00, SalePrice = 1.50 → margen = 50%
        var product = CreateProduct(
            purchasePrice: 1.00m,
            salePrice: 1.50m);

        var margin = product.GetMarginPercent();

        margin.Should().Be(50);
    }

    [Fact]
    public void GetMarginPercent_WhenPurchasePriceIsZero_ShouldReturnZero()
    {
        var product = CreateProduct(
            purchasePrice: 0,
            salePrice: 1.50m);

        var margin = product.GetMarginPercent();

        margin.Should().Be(0);
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithSalePriceLowerThanPurchasePrice_ShouldThrow()
    {
        var action = () => Product.Create(
            name: "Test",
            sku: "TST-001",
            purchasePrice: 10,
            salePrice: 5,     // menor al de compra
            stock: 0,
            minStock: 5,
            categoryId: Guid.NewGuid());

        action.Should().Throw<DomainException>()
            .WithMessage("*precio de venta*");
    }

    [Fact]
    public void Create_WithNegativeStock_ShouldThrow()
    {
        var action = () => Product.Create(
            name: "Test",
            sku: "TST-001",
            purchasePrice: 1,
            salePrice: 2,
            stock: -1,  // negativo
            minStock: 5,
            categoryId: Guid.NewGuid());

        action.Should().Throw<DomainException>();
    }
}