using FluentAssertions;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.UnitTests.Domain;

public class CustomerTests
{
    private static Customer CreateCustomer(
        string fullName = "Juan Pérez",
        decimal creditLimit = 500,
        decimal currentDebt = 0)
    {
        var customer = Customer.Create(
            fullName: fullName,
            phone: null,
            address: null,
            creditLimit: creditLimit);

        // Si necesitamos deuda inicial la simulamos
        if (currentDebt > 0)
            customer.AddDebt(currentDebt);

        return customer;
    }

    // ─── AddDebt ─────────────────────────────────────────────────────────────

    [Fact]
    public void AddDebt_WithValidAmount_ShouldIncreaseDebt()
    {
        var customer = CreateCustomer(creditLimit: 500);

        customer.AddDebt(100);

        customer.CurrentDebt.Should().Be(100);
    }

    [Fact]
    public void AddDebt_ExceedingCreditLimit_ShouldThrow()
    {
        var customer = CreateCustomer(creditLimit: 200, currentDebt: 150);

        var action = () => customer.AddDebt(100); // 150 + 100 = 250 > 200

        action.Should().Throw<CreditLimitExceededException>();
    }

    [Fact]
    public void AddDebt_WithZeroLimit_ShouldAlwaysAllow()
    {
        // CreditLimit = 0 significa sin límite
        var customer = CreateCustomer(creditLimit: 0);

        var action = () => customer.AddDebt(99999);

        action.Should().NotThrow();
    }

    [Fact]
    public void AddDebt_WhenInactive_ShouldThrow()
    {
        var customer = CreateCustomer();
        customer.Deactivate();

        var action = () => customer.AddDebt(100);

        action.Should().Throw<DomainException>()
            .WithMessage("*inactivo*");
    }

    [Fact]
    public void AddDebt_WithNegativeAmount_ShouldThrow()
    {
        var customer = CreateCustomer();

        var action = () => customer.AddDebt(-50);

        action.Should().Throw<DomainException>();
    }

    // ─── RegisterPayment ─────────────────────────────────────────────────────

    [Fact]
    public void RegisterPayment_WithValidAmount_ShouldReduceDebt()
    {
        var customer = CreateCustomer(currentDebt: 200);

        customer.RegisterPayment(50);

        customer.CurrentDebt.Should().Be(150);
    }

    [Fact]
    public void RegisterPayment_ExactDebt_ShouldClearDebt()
    {
        var customer = CreateCustomer(currentDebt: 200);

        customer.RegisterPayment(200);

        customer.CurrentDebt.Should().Be(0);
        customer.HasDebt().Should().BeFalse();
    }

    [Fact]
    public void RegisterPayment_ExceedingDebt_ShouldThrow()
    {
        var customer = CreateCustomer(currentDebt: 100);

        var action = () => customer.RegisterPayment(150);

        action.Should().Throw<DomainException>()
            .WithMessage("*supera la deuda*");
    }

    // ─── GetAvailableCredit ───────────────────────────────────────────────────

    [Fact]
    public void GetAvailableCredit_WithLimit_ShouldReturnDifference()
    {
        var customer = CreateCustomer(creditLimit: 500, currentDebt: 200);

        var available = customer.GetAvailableCredit();

        available.Should().Be(300);
    }

    [Fact]
    public void GetAvailableCredit_WithNoLimit_ShouldReturnNull()
    {
        var customer = CreateCustomer(creditLimit: 0);

        var available = customer.GetAvailableCredit();

        available.Should().BeNull();
    }
}