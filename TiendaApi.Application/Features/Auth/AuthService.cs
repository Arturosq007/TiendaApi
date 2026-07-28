using TiendaApi.Application.Features.Auth.Requests;
using TiendaApi.Application.Features.Auth.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Enums;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Auth;

public class AuthService(
    IUnitOfWork _uow,
    IPasswordHasher _passwordHasher,
    ITokenService _tokenService) : IAuthService
{

    public async Task<AuthResponse> LoginAsync(LoginReq request)
    {
        // 1 — buscar el usuario
        var user = await _uow.Users.GetByUsernameAsync(request.Username);

        if (user is null || !user.IsActive)
            throw new DomainException("Credenciales invalidas.");

        // 2 — verificar contraseña
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Credenciales invalidas.");
        // 3 — generar tokens
        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Username, user.Role.ToString());

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

        // 4 — guardar refresh token hasheado en la DB
        var refreshTokenHash = _passwordHasher.Hash(refreshToken);
        user.SetRefreshToken(refreshTokenHash, refreshTokenExpiry);

        await _uow.SaveChangesAsync();

        // 5 — retornar metadata + tokens para las cookies
        return new AuthResponse
        {
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
            // Los tokens van acá solo para que el controller
            // los ponga en las cookies — nunca llegan al frontend
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterReq request)
    {
        // Validar que el username no exista
        var existingUser = await _uow.Users.GetByUsernameAsync(request.Username);
        if (existingUser is not null)
            throw new DomainException("El nombre de usuario ya está en uso.");
        // Crear nuevo usuario
        var passwordHash = _passwordHasher.Hash(request.Password);

        var newUser = Domain.Entities.User.Create(
            request.Username, request.Name, request.LastName, passwordHash, UserRole.Cajero);
        await _uow.Users.AddAsync(newUser);
        await _uow.SaveChangesAsync();
        // Retornar metadata sin tokens
        return new AuthResponse
        {
            Username = newUser.Username,
            FullName = newUser.FullName,
            Role = newUser.Role.ToString(),
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15)
        };
    }
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // 1 — buscar usuarios con refresh token activo
        // Verificamos contra todos los hashes porque no sabemos
        // a qué usuario pertenece el token
        var users = await _uow.Users.GetAllAsync();
        var user = users.FirstOrDefault(u =>
            u.RefreshTokenHash != null &&
            _passwordHasher.Verify(refreshToken, u.RefreshTokenHash) &&
            u.RefreshTokenExpiry > DateTime.UtcNow) ?? throw new DomainException("Refresh token inválido o expirado.");

        // 2 — generar nuevo par de tokens (rotación)
        var newAccessToken = _tokenService.GenerateAccessToken(
            user.Id, user.Username, user.Role.ToString());

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenExpiry = _tokenService.GetRefreshTokenExpiry();

        // 3 — actualizar el refresh token en la DB
        var newRefreshTokenHash = _passwordHasher.Hash(newRefreshToken);
        user.SetRefreshToken(newRefreshTokenHash, newRefreshTokenExpiry);

        await _uow.SaveChangesAsync();

        return new AuthResponse
        {
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);

        if (user is null) return;

        // Revocar el refresh token — el access token expirará solo
        user.RevokeRefreshToken();
        await _uow.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _uow.Users.GetByIdAsync(userId) ?? throw new NotFoundException("Usuario", userId);

        // Verificar contraseña actual
        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new DomainException("La contraseña actual es incorrecta.");

        // Cambiar contraseña
        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.ChangePassword(newHash);

        // Revocar refresh token — forzar nuevo login
        user.RevokeRefreshToken();

        await _uow.SaveChangesAsync();
    }
}