using FluentAssertions;
using TiendaApi.Application.Features.Customers.Request;
using TiendaApi.Application.Features.Customers.Validation;
using TiendaApi.Application.Features.Products.Request;
using TiendaApi.Application.Features.Products.Validator;
using TiendaApi.Application.Features.Sales.Request;
using TiendaApi.Application.Features.Sales.Validation;
using TiendaApi.Domain.Enums;


namespace TiendaApi.UnitTests.Application;

public class ValidatorTests
{
    // ─── CreateProductValidator ───────────────────────────────────────────────

    [Fact]
    public void CreateProductValidator_WithValidRequest_ShouldPass()
    {
        var validator = new CreateProductValidator();
        var request = new CreateProductRequest
        {
            Name = "Coca Cola",
            PurchasePrice = 1.50m,
            SalePrice = 2.00m,
            InitialStock = 10,
            MinStock = 5,
            CategoryId = Guid.NewGuid()
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateProductValidator_WithSaleLessThanPurchase_ShouldFail()
    {
        var validator = new CreateProductValidator();
        var request = new CreateProductRequest
        {
            Name = "Test",
            PurchasePrice = 10.00m,
            SalePrice = 5.00m, // menor al de compra
            InitialStock = 0,
            MinStock = 5,
            CategoryId = Guid.NewGuid()
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "SalePrice");
    }

    [Fact]
    public void CreateProductValidator_WithEmptyName_ShouldFail()
    {
        var validator = new CreateProductValidator();
        var request = new CreateProductRequest
        {
            Name = "",
            PurchasePrice = 1.00m,
            SalePrice = 2.00m,
            CategoryId = Guid.NewGuid()
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Name");
    }

    [Fact]
    public void CreateProductValidator_WithEmptyCategory_ShouldFail()
    {
        var validator = new CreateProductValidator();
        var request = new CreateProductRequest
        {
            Name = "Test",
            PurchasePrice = 1.00m,
            SalePrice = 2.00m,
            CategoryId = Guid.Empty // vacío
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "CategoryId");
    }

    // ─── CreateSaleValidator ──────────────────────────────────────────────────

    [Fact]
    public void CreateSaleValidator_WithFiadoAndNoCustomer_ShouldFail()
    {
        var validator = new CreateSaleValidator();
        var request = new CreateSaleRequest
        (
            SalePaymentType.Fiado,
            null, // falta el cliente
            Guid.NewGuid(),
            [new SaleDetailRequest
            (
                Guid.NewGuid(),
                1
            )]
        );

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "CustomerId");
    }

    [Fact]
    public void CreateSaleValidator_WithEmptyItems_ShouldFail()
    {
        var validator = new CreateSaleValidator();
        var request = new CreateSaleRequest
        (
            SalePaymentType.Efectivo,
            null,
            Guid.NewGuid(),
            [] // Lista de ítems vacía
        );

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        // La propiedad que falla es "Details"
        result.Errors.Should().Contain(e => e.PropertyName == "Details" && e.ErrorMessage == "La venta no puede estar vacia.");

    }


    [Fact]
    public void CreateSaleValidator_WithZeroQuantity_ShouldFail()
    {
        var validator = new CreateSaleValidator();
        var request = new CreateSaleRequest
        (
            SalePaymentType.Efectivo,
            null,
            Guid.NewGuid(),
            [new SaleDetailRequest
            (
                Guid.NewGuid(),
                0 // cantidad inválida
            )]
        ); 
        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateSaleValidator_WithNegativeDiscount_ShouldFail()
    {
        var validator = new CreateSaleValidator();
        var request = new CreateSaleRequest
        (
            SalePaymentType.Efectivo,
            null,
            Guid.NewGuid(),
            [new SaleDetailRequest
        (
            Guid.NewGuid(),
            -1 // Esto se mapea a Quantity en el validador
        )]
        );

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        // FluentValidation genera el nombre de la propiedad como "Details[0].Quantity"
        result.Errors.Should().Contain(e => e.PropertyName.EndsWith("Quantity"));
    }


    // ─── RegisterPaymentValidator ─────────────────────────────────────────────

    [Fact]
    public void RegisterPaymentValidator_WithZeroAmount_ShouldFail()
    {
        var validator = new RegisterPaymentValidator();
        var request = new RegisterPaymentRequest
        {
            Amount = 0
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Amount");
    }

    [Fact]
    public void RegisterPaymentValidator_WithValidAmount_ShouldPass()
    {
        var validator = new RegisterPaymentValidator();
        var request = new RegisterPaymentRequest
        {
            Amount = 50.00m
        };

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}