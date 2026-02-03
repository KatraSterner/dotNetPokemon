using Microsoft.EntityFrameworkCore;
using Pokemon.Domain;
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

    public Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}