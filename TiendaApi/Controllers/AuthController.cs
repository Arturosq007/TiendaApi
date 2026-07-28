using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiendaApi.Application.Features.Auth;
using TiendaApi.Application.Features.Auth.Requests;
using TiendaApi.Application.Interfaces.Services;

namespace TiendaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : BaseController
{
    // Tiempos de vida de las cookies — deben coincidir con JwtSettings
    private const int AccessTokenExpiryMinutes = 15;
    private const int RefreshTokenExpiryDays = 7;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq request)
    {
        var result = await authService.LoginAsync(request);

        SetAuthCookies(result.AccessToken, result.RefreshToken);

        // Retornamos solo la metadata — nunca los tokens
        return Ok(new
        {
            result.Username,
            result.FullName,
            result.Role,
            result.AccessTokenExpiry
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterReq request)
    {
        var result = await authService.RegisterAsync(request);

        SetAuthCookies(result.AccessToken, result.RefreshToken);

        return Ok(new
        {
            result.Username,
            result.FullName,
            result.Role,
            result.AccessTokenExpiry
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // El refresh token viene automáticamente en la cookie
        var refreshToken = Request.Cookies["refresh_token"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token no encontrado." });

        var result = await authService.RefreshTokenAsync(refreshToken);

        SetAuthCookies(result.AccessToken, result.RefreshToken);

        return Ok(new
        {
            result.Username,
            result.FullName,
            result.Role,
            result.AccessTokenExpiry
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = GetUserIdFromClaims();
        await authService.LogoutAsync(userId);

        // Eliminar ambas cookies
        DeleteAuthCookies();

        return Ok(new { message = "Sesión cerrada correctamente." });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserIdFromClaims();
        await authService.ChangePasswordAsync(userId, request);

        // Cerrar sesión después de cambiar contraseña
        DeleteAuthCookies();

        return Ok(new { message = "Contraseña actualizada. Iniciá sesión nuevamente." });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        // Retorna la info del usuario autenticado desde los claims del JWT
        // No hace consulta a la DB — la info ya está en el token
        return Ok(new
        {
            UserId = GetUserIdFromClaims(),
            Username = User.Identity?.Name,
            Role = User.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }

    // ─── Helpers privados ─────────────────────────────────────────────────────

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        // Opciones base para el Access Token
        var accessTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AccessTokenExpiryMinutes) 
        };

        // Opciones para el Refresh Token (copiamos las base y cambiamos lo necesario)
        var refreshTokenOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            IsEssential = true,
            Expires = DateTimeOffset.UtcNow.AddDays(RefreshTokenExpiryDays),
            Path = "/api/auth/refresh"
        };

        // Guardar en las cookies de la respuesta
        Response.Cookies.Append("access_token", accessToken, accessTokenOptions);
        Response.Cookies.Append("refresh_token", refreshToken, refreshTokenOptions);
    }

    private void DeleteAuthCookies()
    {
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token",
            new CookieOptions { Path = "/api/auth/refresh" });
    }
}
