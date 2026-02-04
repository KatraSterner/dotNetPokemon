using Microsoft.EntityFrameworkCore;
using Pokemon.Domain.Interfaces;
using Pokemon.Infrastructure.Data;

namespace Pokemon.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly PokemonDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(PokemonDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task RemoveAsync(T entity)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<List<T?>> GetByUserIdAsync(int? userId)
    {
        if (userId == null)
            return new List<T?>();

        var property = typeof(T).GetProperty("UserId");
        if (property == null)
            throw new InvalidOperationException($"Type {typeof(T).Name} does not have a UserId property.");

        return await _dbSet
            .Where(e => (int?)property.GetValue(e) == userId)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}