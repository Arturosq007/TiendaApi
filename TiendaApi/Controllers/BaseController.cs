using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace TiendaApi.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("El usuario no está autenticado o el token es inválido.");
    }

    protected bool IsAdmin() => User.IsInRole("Admin");
}
