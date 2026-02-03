namespace Pokemon.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task AddAsync(T entity);
    
    Task<List<T>> GetAllAsync();
    
    Task<T?> GetByIdAsync(int id); 
    
    Task SaveChangesAsync();
}