using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Entities;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username.ToLower().Trim());
    }

    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeId = null)
    {
        return await _dbSet.AnyAsync(u =>
            u.Username == username.ToLower().Trim() &&
            (excludeId == null || u.Id != excludeId));
    }

    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u =>
                u.RefreshTokenHash == tokenHash &&
                u.RefreshTokenExpiry > DateTime.UtcNow);
    }
}