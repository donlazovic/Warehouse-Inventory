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

        var data = Validate(request);

        if (await repo.ExistsAsync(x => x.Name == data.Name, ct))
            throw new AppException("Dobavljac sa istim nazivom vec postoji.");

        var supplier = new Supplier { IsActive = request.IsActive };
        Apply(supplier, data);

        await repo.AddAsync(supplier, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(supplier.Id, ct);
    }

    public async Task<SupplierDto> UpdateAsync(int id, SaveSupplierRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Supplier>();

        var supplier = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Dobavljac nije pronadjen.", 404);

        var data = Validate(request);

        if (await repo.ExistsAsync(x => x.Name == data.Name && x.Id != id, ct))
            throw new AppException("Dobavljac sa istim nazivom vec postoji.");

        Apply(supplier, data);
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

    private static Supplier Validate(SaveSupplierRequest request)
    {
        var taxNumber = Guard.Optional(request.TaxNumber, "PIB", 20);

        if (taxNumber is not null && (taxNumber.Length != 9 || !taxNumber.All(char.IsDigit)))
            throw new AppException("PIB mora imati tacno 9 cifara.");

        return new Supplier
        {
            Name = Guard.Required(request.Name, "Naziv", 200),
            TaxNumber = taxNumber,
            ContactPerson = Guard.Optional(request.ContactPerson, "Kontakt osoba", 150),
            Email = Guard.Email(request.Email),
            Phone = Guard.Optional(request.Phone, "Telefon", 50),
            Address = Guard.Optional(request.Address, "Adresa", 300),
            City = Guard.Optional(request.City, "Grad", 100)
        };
    }

    private static void Apply(Supplier target, Supplier data)
    {
        target.Name = data.Name;
        target.TaxNumber = data.TaxNumber;
        target.ContactPerson = data.ContactPerson;
        target.Email = data.Email;
        target.Phone = data.Phone;
        target.Address = data.Address;
        target.City = data.City;
    }

    private static IQueryable<SupplierDto> Project(IQueryable<Supplier> query)
        => query.Select(x => new SupplierDto(
            x.Id, x.Name, x.TaxNumber, x.ContactPerson, x.Email, x.Phone, x.Address, x.City, x.IsActive,
            x.Orders.Count,
            x.Orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => (decimal?)o.TotalValue) ?? 0m,
            x.Orders.Max(o => (DateTime?)o.CreatedAt),
            x.CreatedAt));
}
