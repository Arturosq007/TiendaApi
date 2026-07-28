using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TiendaApi.Domain.Common;
using TiendaApi.Domain.Interfaces.Repositories;
using TiendaApi.Infrastructure.Data;

namespace TiendaApi.Infrastructure.Repositories;

public class GenericRepository<T>(AppDbContext context) : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context = context; // Sirve para operaciones de escritura (SaveChanges)
    protected readonly DbSet<T> _dbSet = context.Set<T>(); // Sirve para consultas (LINQ, FindAsync, etc.)

    // ─── Consultas ────────────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        return predicate is null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
    }

    // ─── Escritura ────────────────────────────────────────────────────────────

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    // sirve para agregar varias entidades en una sola operación, lo que puede ser más eficiente que agregar una por una
    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
    }

    public void Update(T entity)
    {
        // Attach registra la entidad en el ChangeTracker si no está
        // Mark la como Modified para que EF Core genere el UPDATE
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}