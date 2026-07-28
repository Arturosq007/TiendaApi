using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class CashRegister : BaseEntity
{
    public DateTime OpenedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; private set; }
    public decimal OpeningBalance { get; private set; } // Monto inicial de caja
    public decimal? ClosingBalance { get; private set; } // Monto final de caja al cerrar
    public decimal? ExpectedBalance { get; private set; } // Monto esperado de caja al cerrar
    public decimal? Difference { get; private set; }
    public bool IsOpen { get; private set; } = true;
    public string? Notes { get; private set; }
    public Guid UserId { get; private set; }

    // Navegación
    public User User { get; set; } = null!;
    public ICollection<Sale> Sales { get; private set; } = [];
    private CashRegister() { }
    public static CashRegister Create(Guid userId, decimal openingBalance, string? notes = null)
    {
        return new CashRegister(userId, openingBalance, notes);
    }

    private CashRegister(Guid userId, decimal openingBalance, string? notes)
    {
        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        if (openingBalance < 0)
            throw new DomainException("El monto inicial de caja no puede ser negativo.");

        UserId = userId;
        OpeningBalance = openingBalance;
        Notes = notes?.Trim();
    }

    public bool CanBeClosed() => IsOpen;
    public void Close(decimal closingBalance, decimal expectedBalance)
    {
        if (!CanBeClosed())
            throw new InvalidEntityStateException("Caja", "Cerrada", "cerrar");

        if (closingBalance < 0)
            throw new DomainException("El monto de cierre no puede ser negativo.");

        ClosingBalance = closingBalance;
        ExpectedBalance = expectedBalance;
        Difference = closingBalance - expectedBalance;
        ClosedAt = DateTime.UtcNow;
        IsOpen = false;
    }

    public void AddNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new DomainException("Las notas no pueden estar vacías.");

        Notes = notes.Trim();
    }
    public bool HasDifference() => Difference.HasValue && Difference.Value != 0;
    public bool HasShortage() => Difference.HasValue && Difference.Value < 0;
    public bool HasSurplus() => Difference.HasValue && Difference.Value > 0;
}