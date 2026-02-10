namespace Pokemon.Domain.Interfaces;

public interface IUserOwnedRepository<T> : IRepository<T> where T : class, IUserOwnedEntity
{
    Task<List<T>> GetByUserIdAsync(int userId);
}