using TiendaApi.Domain.Common;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;

public class Supplier : ActivatableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? ContactEmail { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public ICollection<Product> Products { get; private set; } = [];
    public ICollection<Purchase> Purchases { get; private set; } = [];

    private Supplier() { }

    public static Supplier Create(string name, string? contactEmail = null,
        string? phone = null, string? address = null)
    {
        return new Supplier(name, contactEmail, phone, address);
    }

    private Supplier(string name, string? contactEmail, string? phone, string? address)
    {
        Name = ValidateName(name);
        ContactEmail = ValidateEmail(contactEmail);
        Phone = phone?.Trim();
        Address = address?.Trim();
    }

    public void Update(string name, string? contactEmail, string? phone, string? address)
    {
        Name = ValidateName(name);
        ContactEmail = ValidateEmail(contactEmail);
        Phone = phone?.Trim();
        Address = address?.Trim();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del proveedor es obligatorio.");

        if (name.Trim().Length > 150)
            throw new DomainException("El nombre no puede superar los 150 caracteres.");

        return name.Trim();
    }

    private static string? ValidateEmail(string? email)
    {
        if (email is null) return null;

        email = email.Trim();

        if (!email.Contains('@'))
            throw new DomainException("El email del proveedor no es válido.");

        return email;
    }
}