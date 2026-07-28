using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class CreditPayment : BaseEntity
{
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid UserId { get; private set; }
    // Navegación
    public Customer Customer { get; set; } = null!;
    public User User { get; set; } = null!;

    private CreditPayment() { }
    public static CreditPayment Create(Guid customerId, Guid userId,
        decimal amount, string? notes = null)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("El cliente es obligatorio.");

        if (userId == Guid.Empty)
            throw new DomainException("El usuario es obligatorio.");

        if (amount <= 0)
            throw new DomainException("El monto del pago debe ser mayor a cero.");

        return new CreditPayment(customerId, userId, amount, notes);
    }
    private CreditPayment(Guid customerId, Guid userId, decimal amount, string? notes)
    {
        CustomerId = customerId;
        UserId = userId;
        Amount = amount;
        Notes = notes?.Trim();
    }
}