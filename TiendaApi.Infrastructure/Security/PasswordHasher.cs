using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    // El work factor define cuántas rondas de hashing se hacen.
    // Más alto = más seguro pero más lento.
    // 12 es el estándar recomendado actualmente.
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        // Cambia 'BCrypt.HashPassword' por 'BCrypt.Net.BCrypt.HashPassword'
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        // Cambia 'BCrypt.Verify' por 'BCrypt.Net.BCrypt.Verify'
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}