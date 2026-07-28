namespace TiendaApi.Application.Features.Auth.Requests;

public record class RegisterReq
{
    public string Name { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
