using FluentAssertions;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.UnitTests.Domain;

public class CashRegisterTests
{
    private static CashRegister CreateSession(
        decimal openingBalance = 100.00m)
    {
        return CashRegister.Create(
            userId: Guid.NewGuid(),
            openingBalance: openingBalance);
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidData_ShouldBeOpen()
    {
        var session = CreateSession(100.00m);

        session.IsOpen.Should().BeTrue();
        session.OpeningBalance.Should().Be(100.00m);
        session.ClosedAt.Should().BeNull();
        session.Difference.Should().BeNull();
    }

    [Fact]
    public void Create_WithNegativeBalance_ShouldThrow()
    {
        var action = () => CreateSession(-50.00m);

        action.Should().Throw<DomainException>()
            .WithMessage("*negativo*");
    }

    [Fact]
    public void Create_WithEmptyUser_ShouldThrow()
    {
        var action = () => CashRegister.Create(
            userId: Guid.Empty,
            openingBalance: 100.00m);

        action.Should().Throw<DomainException>()
            .WithMessage("*usuario*");
    }

    // ─── Close ───────────────────────────────────────────────────────────────

    [Fact]
    public void Close_WithExactBalance_ShouldHaveZeroDifference()
    {
        var session = CreateSession(100.00m);

        // Expected = 150 (100 inicial + 50 en ventas)
        session.Close(
            closingBalance: 150.00m,
            expectedBalance: 150.00m);

        session.IsOpen.Should().BeFalse();
        session.Difference.Should().Be(0);
        session.ClosedAt.Should().NotBeNull();
        session.HasDifference().Should().BeFalse();
    }

    [Fact]
    public void Close_WithShortage_ShouldHaveNegativeDifference()
    {
        var session = CreateSession(100.00m);

        // Debería haber 150 pero hay 130 → faltante de 20
        session.Close(
            closingBalance: 130.00m,
            expectedBalance: 150.00m);

        session.Difference.Should().Be(-20.00m);
        session.HasShortage().Should().BeTrue();
        session.HasSurplus().Should().BeFalse();
    }

    [Fact]
    public void Close_WithSurplus_ShouldHavePositiveDifference()
    {
        var session = CreateSession(100.00m);

        // Debería haber 150 pero hay 170 → sobrante de 20
        session.Close(
            closingBalance: 170.00m,
            expectedBalance: 150.00m);

        session.Difference.Should().Be(20.00m);
        session.HasSurplus().Should().BeTrue();
        session.HasShortage().Should().BeFalse();
    }

    [Fact]
    public void Close_AlreadyClosed_ShouldThrow()
    {
        var session = CreateSession();
        session.Close(100.00m, 100.00m);

        var action = () => session.Close(100.00m, 100.00m);

        action.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void Close_WithNegativeClosingBalance_ShouldThrow()
    {
        var session = CreateSession();

        var action = () => session.Close(-50.00m, 100.00m);

        action.Should().Throw<DomainException>()
            .WithMessage("*negativo*");
    }

    // ─── CanBeClosed ─────────────────────────────────────────────────────────

    [Fact]
    public void CanBeClosed_WhenOpen_ShouldReturnTrue()
    {
        var session = CreateSession();

        session.CanBeClosed().Should().BeTrue();
    }

    [Fact]
    public void CanBeClosed_WhenClosed_ShouldReturnFalse()
    {
        var session = CreateSession();
        session.Close(100.00m, 100.00m);

        session.CanBeClosed().Should().BeFalse();
    }
}