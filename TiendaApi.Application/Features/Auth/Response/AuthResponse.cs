using System.Text.Json.Serialization;

namespace TiendaApi.Application.Features.Auth.Response;

public record class AuthResponse
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Cuándo expira el access token — el frontend lo usa
    // para saber cuándo pedir un nuevo token
    public DateTime AccessTokenExpiry { get; set; }
    [JsonIgnore]
    public string AccessToken { get; set; } = string.Empty;
    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}