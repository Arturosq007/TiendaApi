using FluentAssertions;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.UnitTests.Domain;

public class PurchaseTests
{
    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Purchase CreatePurchase(
        Guid? supplierId = null,
        PurchasePaymentType paymentType = PurchasePaymentType.Contado)
    {
        return Purchase.Create(
            supplierId: supplierId ?? Guid.NewGuid(),
            userId: Guid.NewGuid(),
            paymentType: paymentType);
    }

    private static (Guid ProductId, int Quantity, decimal UnitCost) CreateItem(
        int quantity = 5,
        decimal unitCost = 10.00m)
    {
        return (Guid.NewGuid(), quantity, unitCost);
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldCreatePendingPurchase()
    {
        var purchase = CreatePurchase();

        purchase.Status.Should().Be(PurchaseStatus.Pendiente);
        purchase.Total.Should().Be(0);
        purchase.Details.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyUser_ShouldThrow()
    {
        var action = () => Purchase.Create(
            supplierId: Guid.NewGuid(),
            userId: Guid.Empty,
            paymentType: PurchasePaymentType.Contado);

        action.Should().Throw<DomainException>()
            .WithMessage("*usuario*");
    }

    // ─── AddDetail ───────────────────────────────────────────────────────────

    [Fact]
    public void AddDetail_WithValidData_ShouldAddAndRecalculateTotal()
    {
        var purchase = CreatePurchase();
        var (ProductId, Quantity, UnitCost) = CreateItem(quantity: 5, unitCost: 10.00m);

        purchase.AddDetails([(ProductId, Quantity, UnitCost)]);

        purchase.Details.Should().HaveCount(1);
        purchase.Total.Should().Be(50.00m); // 5 * 10
    }

    [Fact]
    public void AddDetail_MultipleItems_ShouldAccumulateTotal()
    {
        var purchase = CreatePurchase();

        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m), (Guid.NewGuid(), 3, 20.00m)]);

        purchase.Total.Should().Be(110.00m); // (5*10) + (3*20)
        purchase.Details.Should().HaveCount(2);
    }

    [Fact]
    public void AddDetail_WithZeroQuantity_ShouldThrow()
    {
        var purchase = CreatePurchase();

        var action = () => purchase.AddDetails([(Guid.NewGuid(), 0, 10.00m)]);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddDetail_WithZeroCost_ShouldThrow()
    {
        var purchase = CreatePurchase();

        var action = () => purchase.AddDetails([(Guid.NewGuid(), 5, 0)]);

        action.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddDetail_ToReceivedPurchase_ShouldThrow()
    {
        var purchase = CreatePurchase();
        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m)]);
        purchase.Receive();

        var action = () => purchase.AddDetails([(Guid.NewGuid(), 1, 5.00m)]);

        action.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void AddDetail_ToCancelledPurchase_ShouldThrow()
    {
        var purchase = CreatePurchase();
        purchase.Cancel();

        var action = () => purchase.AddDetails([(Guid.NewGuid(), 1, 5.00m)]);

        action.Should().Throw<InvalidEntityStateException>();
    }

    // ─── Receive ─────────────────────────────────────────────────────────────

    [Fact]
    public void Receive_WhenPending_ShouldChangeToReceived()
    {
        var purchase = CreatePurchase();
        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m)]);

        purchase.Receive();

        purchase.Status.Should().Be(PurchaseStatus.Recibida);
        purchase.IsReceived().Should().BeTrue();
    }

    [Fact]
    public void Receive_WhenAlreadyReceived_ShouldThrow()
    {
        var purchase = CreatePurchase();
        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m)]);
        purchase.Receive();

        var action = () => purchase.Receive();

        action.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void Receive_WhenCancelled_ShouldThrow()
    {
        var purchase = CreatePurchase();
        purchase.Cancel();

        var action = () => purchase.Receive();

        action.Should().Throw<InvalidEntityStateException>();
    }

    // ─── Cancel ──────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_WhenPending_ShouldChangeToCancelled()
    {
        var purchase = CreatePurchase();

        purchase.Cancel();

        purchase.Status.Should().Be(PurchaseStatus.Cancelada);
        purchase.IsCancelled().Should().BeTrue();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrow()
    {
        var purchase = CreatePurchase();
        purchase.Cancel();

        var action = () => purchase.Cancel();

        action.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void Cancel_WhenReceived_ShouldThrow()
    {
        var purchase = CreatePurchase();
        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m)]);
        purchase.Receive();

        var action = () => purchase.Cancel();

        action.Should().Throw<InvalidEntityStateException>();
    }

    // ─── UpdateInvoiceNumber ─────────────────────────────────────────────────

    [Fact]
    public void UpdateInvoiceNumber_WithValidNumber_ShouldUpdate()
    {
        var purchase = CreatePurchase();

        purchase.UpdateInvoiceNumber("FAC-001");

        purchase.InvoiceNumber.Should().Be("FAC-001");
    }

    [Fact]
    public void UpdateInvoiceNumber_WithEmpty_ShouldThrow()
    {
        var purchase = CreatePurchase();

        var action = () => purchase.UpdateInvoiceNumber(string.Empty);

        action.Should().Throw<DomainException>();
    }

    // ─── Estado ──────────────────────────────────────────────────────────────

    [Fact]
    public void IsPending_WhenJustCreated_ShouldReturnTrue()
    {
        var purchase = CreatePurchase();

        purchase.IsPending().Should().BeTrue();
        purchase.CanBeReceived().Should().BeTrue();
        purchase.CanBeCancelled().Should().BeTrue();
    }

    [Fact]
    public void IsReceived_AfterReceive_ShouldReturnTrue()
    {
        var purchase = CreatePurchase();
        purchase.AddDetails([(Guid.NewGuid(), 5, 10.00m)]);
        purchase.Receive();

        purchase.IsReceived().Should().BeTrue();
        purchase.CanBeReceived().Should().BeFalse();
        purchase.CanBeCancelled().Should().BeFalse();
    }
}