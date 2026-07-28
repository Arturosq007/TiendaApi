namespace TiendaApi.Application.Features.Auth.Requests;

public record class LoginReq
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}