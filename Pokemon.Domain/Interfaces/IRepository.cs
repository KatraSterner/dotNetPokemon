namespace Pokemon.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    
    Task RemoveAsync(T entity);
    
    Task<List<T>> GetAllAsync();
    
    Task<T?> GetByIdAsync(int id); 
    
    // Task<List<T?>> GetByUserIdAsync(int userId);
    
    Task SaveChangesAsync();
}