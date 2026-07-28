using TiendaApi.Application.Features.Users.Requests;
using TiendaApi.Application.Features.Users.Response;
using TiendaApi.Application.Interfaces.Services;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Exceptions;
using TiendaApi.Domain.Interfaces;

namespace TiendaApi.Application.Features.Users;

public class UserService(IUnitOfWork _uow /*, IPasswordHasher _passwordHasher*/) : IUserService
{

    public async Task<IReadOnlyCollection<UserResponse>> GetAllAsync()
    {
        var users = await _uow.Users.GetAllAsync();
        return [..users.Select(MapToDto)];
    }

    public async Task<UserResponse> GetByIdAsync(Guid id)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("Usuario", id);

        return MapToDto(user);
    }

    //public async Task<UserResponse> CreateAsync(CreateUserRequest request)
    //{
    //    // Verificar que el username no esté en uso
    //    var usernameExists = await _uow.Users
    //        .UsernameExistsAsync(request.Username);

    //    if (usernameExists)
    //        throw new DomainException(
    //            $"El usuario '{request.Username}' ya está en uso.");

    //    var user = User.Create(
    //        username: request.Username,
    //        name: request.Name,
    //        lastName: request.LastName,
    //        passwordHash: _passwordHasher.Hash(request.Password),
    //        role: request.Role);

    //    await _uow.Users.AddAsync(user);
    //    await _uow.SaveChangesAsync();

    //    return MapToDto(user);
    //}

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("Usuario", id);

        user.UpdateProfile(request.Name, request.LastName);
        user.ChangeRole(request.Role);

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task ToggleActiveAsync(Guid id, Guid requestingUserId)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException("Usuario", id);

        // Un usuario no puede desactivarse a sí mismo
        if (id == requestingUserId)
            throw new DomainException(
                "No podés desactivar tu propio usuario.");

        if (user.IsActive)
            user.Deactivate();
        else
            user.Activate();

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();
    }

    // ─── Mapeo ────────────────────────────────────────────────────────────────

    private static UserResponse MapToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Name = user.Name,
        LastName = user.LastName,
        FullName = user.FullName,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}