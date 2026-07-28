namespace TiendaApi.Domain.Interfaces;
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string username, string role);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
    DateTime GetRefreshTokenExpiry();
}