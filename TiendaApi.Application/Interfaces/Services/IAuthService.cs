using TiendaApi.Application.Features.Auth;
using TiendaApi.Application.Features.Auth.Requests;
using TiendaApi.Application.Features.Auth.Response;

namespace TiendaApi.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginReq request);
    Task<AuthResponse> RegisterAsync(RegisterReq request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}