using TiendaApi.Domain.Common;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;

namespace TiendaApi.Domain.Entities;
public class User : ActivatableEntity
{
    public string Username { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    public ICollection<Sale> Sales { get; private set; } = [];
    public ICollection<Purchase> Purchases { get; private set; } = [];
    public ICollection<StockMovement> StockMovements { get; private set; } = [];
    public ICollection<CreditPayment> CreditPayments { get; private set; } = [];

    private User() { }
    public static User Create(string username, string name, string lastName,
        string passwordHash, UserRole role)
    {
        return new User(username, name, lastName, passwordHash,role);
    }
    private User(string username, string name, string lastName,
        string passwordHash, UserRole role)
    {
        Username = ValidateUsername(username);
        Name = ValidateName(name, "El nombre");
        LastName = ValidateName(lastName, "El apellido");
        PasswordHash = ValidatePasswordHash(passwordHash);
        Role = role;
    }

    public string FullName => $"{Name} {LastName}";
    public bool IsAdmin() => Role == UserRole.Admin;
    public void UpdateProfile(string name, string lastName)
    {
        Name = ValidateName(name, "El nombre");
        LastName = ValidateName(lastName, "El apellido");
    }
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = ValidatePasswordHash(newPasswordHash);
    }
    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    public bool HasValidRefreshToken(string tokenHash) =>
        RefreshTokenHash == tokenHash &&
        RefreshTokenExpiry.HasValue &&
        RefreshTokenExpiry.Value > DateTime.UtcNow;

    public void SetRefreshToken(string tokenHash, DateTime expiry)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("El refresh token no puede estar vacío.");

        if (expiry <= DateTime.UtcNow)
            throw new DomainException("La fecha de expiración debe ser futura.");

        RefreshTokenHash = tokenHash;
        RefreshTokenExpiry = expiry;
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiry = null;
    }

    // ─── Validaciones 
    private static string ValidateUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("El nombre de usuario es obligatorio.");

        if (username.Trim().Length < 3)
            throw new DomainException("El nombre de usuario debe tener al menos 3 caracteres.");

        if (username.Trim().Length > 50)
            throw new DomainException("El nombre de usuario no puede superar los 50 caracteres.");

        return username.Trim().ToLower();
    }
    private static string ValidateName(string name, string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException($"{fieldLabel} es obligatorio.");

        if (name.Trim().Length > 100)
            throw new DomainException($"{fieldLabel} no puede superar los 100 caracteres.");

        return name.Trim();
    }
    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("La contraseña es obligatoria.");

        return passwordHash;
    }
}