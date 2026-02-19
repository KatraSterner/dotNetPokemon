using Microsoft.EntityFrameworkCore;
using Pokemon.Domain.Interfaces;
using Pokemon.Infrastructure.Data;

namespace Pokemon.Infrastructure.Repositories;

public class UserOwnedRepository<T>
    : Repository<T>, IUserOwnedRepository<T>
    where T : class, IUserOwnedEntity
{
    public UserOwnedRepository(PokemonDbContext context)
        : base(context) { }

    public async Task<List<T>> GetByUserIdAsync(int userId)
    {
        
        if (userId <= 0)
            throw new ArgumentException("UserId must be greater than 0.", nameof(userId));

        return await _dbSet
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }
}
