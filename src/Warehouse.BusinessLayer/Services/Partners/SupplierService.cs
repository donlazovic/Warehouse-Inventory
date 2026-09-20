using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Partners;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Partners;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _uow;

    public SupplierService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<SupplierDto>> GetPagedAsync(SupplierFilterRequest filter, CancellationToken ct = default)
    {
        var query = _uow.Repository<Supplier>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.Name.Contains(term)
                                  || (x.ContactPerson != null && x.ContactPerson.Contains(term))
                                  || (x.TaxNumber != null && x.TaxNumber.Contains(term)));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(x => x.City == filter.City);

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "city" => query.ApplySort(x => x.City, filter.SortDesc),
            "createdat" => query.ApplySort(x => x.CreatedAt, filter.SortDesc),
            _ => query.ApplySort(x => x.Name, filter.SortDesc)
        };

        return await Project(query).ToPagedResultAsync(filter, ct);
    }

    public async Task<SupplierDto> GetByIdAsync(int id, CancellationToken ct = default)
        => await Project(_uow.Repository<Supplier>().Query().Where(x => x.Id == id)).FirstOrDefaultAsync(ct)
           ?? throw new AppException("Dobavljac nije pronadjen.", 404);

    public async Task<SupplierDto> CreateAsync(SaveSupplierRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Supplier>();

        if (await repo.ExistsAsync(x => x.Name == request.Name, ct))
            throw new AppException("Dobavljac sa istim nazivom vec postoji.");

        var supplier = new Supplier
        {
            Name = request.Name.Trim(),
            TaxNumber = request.TaxNumber?.Trim(),
            ContactPerson = request.ContactPerson?.Trim(),
            Email = request.Email?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            IsActive = request.IsActive
        };

        await repo.AddAsync(supplier, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(supplier.Id, ct);
    }

    public async Task<SupplierDto> UpdateAsync(int id, SaveSupplierRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Supplier>();

        var supplier = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Dobavljac nije pronadjen.", 404);

        if (await repo.ExistsAsync(x => x.Name == request.Name && x.Id != id, ct))
            throw new AppException("Dobavljac sa istim nazivom vec postoji.");

        supplier.Name = request.Name.Trim();
        supplier.TaxNumber = request.TaxNumber?.Trim();
        supplier.ContactPerson = request.ContactPerson?.Trim();
        supplier.Email = request.Email?.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Address = request.Address?.Trim();
        supplier.City = request.City?.Trim();
        supplier.IsActive = request.IsActive;

        repo.Update(supplier);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Supplier>();

        var supplier = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Dobavljac nije pronadjen.", 404);

        if (await _uow.Repository<Order>().ExistsAsync(x => x.SupplierId == id, ct))
            throw new AppException("Dobavljac se ne moze obrisati jer postoje nalozi. Deaktivirajte ga umesto toga.");

        repo.Remove(supplier);
        await _uow.SaveChangesAsync(ct);
    }

    private static IQueryable<SupplierDto> Project(IQueryable<Supplier> query)
        => query.Select(x => new SupplierDto(
            x.Id, x.Name, x.TaxNumber, x.ContactPerson, x.Email, x.Phone, x.Address, x.City, x.IsActive,
            x.Orders.Count,
            x.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => (decimal?)o.TotalValue) ?? 0m,
            x.Orders.Max(o => (DateTime?)o.CreatedAt),
            x.CreatedAt));
}
