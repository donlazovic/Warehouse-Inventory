using Microsoft.EntityFrameworkCore.Storage;
using Warehouse.Domain.Common;

namespace Warehouse.DataLayer.Repositories;

public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
