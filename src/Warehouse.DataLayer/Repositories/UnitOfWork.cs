using Microsoft.EntityFrameworkCore.Storage;
using Warehouse.DataLayer.Context;
using Warehouse.Domain.Common;

namespace Warehouse.DataLayer.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WarehouseDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(WarehouseDbContext context) => _context = context;

    public IRepository<T> Repository<T>() where T : BaseEntity
    {
        if (_repositories.TryGetValue(typeof(T), out var existing))
            return (IRepository<T>)existing;

        var repository = new Repository<T>(_context);
        _repositories[typeof(T)] = repository;
        return repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => _context.Database.BeginTransactionAsync(ct);
}
