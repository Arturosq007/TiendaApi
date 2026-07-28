using TiendaApi.Domain.Enums;

namespace TiendaApi.Application.Features.Users.Requests;

public class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}