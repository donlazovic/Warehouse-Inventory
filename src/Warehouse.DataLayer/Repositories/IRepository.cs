using System.Linq.Expressions;
using Warehouse.Domain.Common;

namespace Warehouse.DataLayer.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    IQueryable<T> Query(bool asNoTracking = true);
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
