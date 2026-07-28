using TiendaApi.Application.Features.Users.Requests;
using TiendaApi.Application.Features.Users.Response;

namespace TiendaApi.Application.Interfaces.Services;

public interface IUserService
{
    Task<IReadOnlyCollection<UserResponse>> GetAllAsync();
    Task<UserResponse> GetByIdAsync(Guid id);
    //Task<UserResponse> CreateAsync(CreateUserRequest request);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
    Task ToggleActiveAsync(Guid id, Guid requestingUserId);
}