using Pokemon.Domain;

namespace Pokemon.Infrastructure;
using Microsoft.EntityFrameworkCore;


public class Repository<T> : IRepository<T> where T : class
{
    private readonly ChoreDbContext _context;

    private readonly DbSet<T> _dbSet;

    public Repository(ChoreDbContext context)
    {
        _context = context;

        _dbSet = _context.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

}