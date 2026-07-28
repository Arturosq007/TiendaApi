using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;

public class Customer : ActivatableEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public decimal CreditLimit { get; private set; } = 0;
    public decimal CurrentDebt { get; private set; } = 0;

    public IEnumerable<Sale> Sales { get; private set; } = [];
    public IEnumerable<CreditPayment> CreditPayments { get; private set; } = [];

    private Customer() { }
    public static Customer Create(string fullName, string? phone,
        string? address, decimal creditLimit)
    {
        return new Customer(fullName, phone, address, creditLimit);
    }
    private Customer(string fullName, string? phone, string? address, decimal creditLimit)
    {
        FullName = ValidateFullName(fullName);
        Phone = phone?.Trim();
        Address = address?.Trim();
        CreditLimit = ValidateCreditLimit(creditLimit);
    }

    public void Update(string fullName, string? phone, string? address, decimal creditLimit)
    {
        FullName = ValidateFullName(fullName);
        Phone = phone?.Trim();
        Address = address?.Trim();
        CreditLimit = ValidateCreditLimit(creditLimit);
    }
    public bool CanAddDebt(decimal amount)
    {
        if (!IsActive)
            return false;

        if (CreditLimit == 0) return true;

        return (CurrentDebt + amount) <= CreditLimit;
    }
    public void AddDebt(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("El monto de deuda debe ser mayor a cero.");

        if (!IsActive)
            throw new DomainException($"El cliente '{FullName}' está inactivo y no puede recibir nuevos fiados.");

        if (!CanAddDebt(amount))
            throw new CreditLimitExceededException(FullName, CreditLimit, CurrentDebt, amount);

        CurrentDebt += amount;
    }

    public void DecreaseDebt(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("El monto de reducción de deuda debe ser mayor a cero.");
        if (amount > CurrentDebt)
            throw new DomainException(
                $"La reducción de deuda ({amount:C}) supera la deuda actual ({CurrentDebt:C}).");
        CurrentDebt -= amount;
    }
    public void ClearDebt()
    {
        CurrentDebt = 0;
    }
    public void RegisterPayment(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("El monto del pago debe ser mayor a cero.");

        if (amount > CurrentDebt)
            throw new DomainException(
                $"El pago ({amount:C}) supera la deuda actual ({CurrentDebt:C}).");

        CurrentDebt -= amount;
    }
    public bool HasDebt() => CurrentDebt > 0;
    public decimal? GetAvailableCredit()
    {
        if (CreditLimit == 0) return null;
        return Math.Max(0, CreditLimit - CurrentDebt);
    }

    private static string ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("El nombre completo es obligatorio.");

        if (fullName.Trim().Length > 150)
            throw new DomainException("El nombre no puede superar los 150 caracteres.");

        return fullName.Trim();
    }
    private static decimal ValidateCreditLimit(decimal creditLimit)
    {
        if (creditLimit < 0)
            throw new DomainException("El límite de crédito no puede ser negativo.");

        return creditLimit;
    }
}