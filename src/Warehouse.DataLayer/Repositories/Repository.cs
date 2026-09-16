using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Warehouse.DataLayer.Context;
using Warehouse.Domain.Common;

namespace Warehouse.DataLayer.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly WarehouseDbContext Context;
    protected readonly DbSet<T> Set;

    public Repository(WarehouseDbContext context)
    {
        Context = context;
        Set = context.Set<T>();
    }

    public IQueryable<T> Query(bool asNoTracking = true)
        => asNoTracking ? Set.AsNoTracking() : Set;

    public Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => Set.AsNoTracking().ToListAsync(ct);

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => Set.AnyAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await Set.AddAsync(entity, ct);

    public void Update(T entity) => Set.Update(entity);

    public void Remove(T entity) => Set.Remove(entity);
}
