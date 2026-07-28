using FluentAssertions;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.UnitTests.Domain;

public class SaleTests
{
    private static Sale CreateSale(
        SalePaymentType paymentType = SalePaymentType.Efectivo,
        Guid? customerId = null)
    {
        return Sale.Create(
            userId: Guid.NewGuid(),
            paymentType: paymentType,
            customerId: customerId);
    }

    // ─── AddDetail ───────────────────────────────────────────────────────────

    [Fact]
    public void AddDetail_WithValidData_ShouldAddDetailAndRecalculate()
    {
        var sale = CreateSale();

        sale.AddDetails([(Guid.NewGuid(), 2, 5.00m)]);

        sale.Details.Should().HaveCount(1);
        sale.Subtotal.Should().Be(8.47m);
        sale.IGV.Should().Be(1.53m);
        sale.Total.Should().Be(10.00m);
    }

    [Fact]
    public void AddDetail_MultipleItems_ShouldAccumulateTotals()
    {
        var sale = CreateSale();

        sale.AddDetails([(Guid.NewGuid(), 2, 10.00m), (Guid.NewGuid(), 1, 5.00m)]);

        sale.Subtotal.Should().Be(21.19m);
        sale.Total.Should().Be(25.00m); // 25 + IGV 18%
    }

    [Fact]
    public void AddDetail_WithZeroQuantity_ShouldThrow()
    {
        var sale = CreateSale();

        var action = () => sale.AddDetails([(Guid.NewGuid(), 0, 5.00m)]);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddDetail_WithZeroPrice_ShouldThrow()
    {
        var sale = CreateSale();

        var action = () => sale.AddDetails([(Guid.NewGuid(), 1, 0)]);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddDetail_ToCancelledSale_ShouldThrow()
    {
        var sale = CreateSale();
        sale.Cancel();

        var action = () => sale.AddDetails([(Guid.NewGuid(), 1, 5.00m)]);

        action.Should().Throw<InvalidEntityStateException>();
    }

    // ─── Cancel ──────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenCompleted_ShouldChangeToCancelled()
    {
        var sale = CreateSale();

        sale.Cancel();

        sale.Status.Should().Be(SaleStatus.Cancelada);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldThrow()
    {
        var sale = CreateSale();
        sale.Cancel();

        var action = () => sale.Cancel();

        action.Should().Throw<InvalidEntityStateException>();
    }

    // ─── IsCredit ────────────────────────────────────────────────────────────

    [Fact]
    public void IsCredit_WithFiado_ShouldReturnTrue()
    {
        var sale = CreateSale(SalePaymentType.Fiado, Guid.NewGuid());

        sale.IsCredit().Should().BeTrue();
    }

    [Fact]
    public void IsCredit_WithEfectivo_ShouldReturnFalse()
    {
        var sale = CreateSale(SalePaymentType.Efectivo);

        sale.IsCredit().Should().BeFalse();
    }

    // ─── IGV ─────────────────────────────────────────────────────────────────

    [Fact]
    public void AddDetail_ShouldCalculateIGVCorrectly()
    {
        var sale = CreateSale();

        // Subtotal = 100.00
        sale.AddDetails([(Guid.NewGuid(), 10, 10.00m)]);

        sale.IGV.Should().Be(15.25m);      // 18%
        sale.Total.Should().Be(100.00m);   // 100 + 18
    }
}