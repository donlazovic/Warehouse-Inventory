using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Partners;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Partners;

public class StoreService : IStoreService
{
    private readonly IUnitOfWork _uow;

    public StoreService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<StoreDto>> GetPagedAsync(StoreFilterRequest filter, CancellationToken ct = default)
    {
        var query = _uow.Repository<Store>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(x => x.City == filter.City);

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "code" => query.ApplySort(x => x.Code, filter.SortDesc),
            "city" => query.ApplySort(x => x.City, filter.SortDesc),
            "createdat" => query.ApplySort(x => x.CreatedAt, filter.SortDesc),
            _ => query.ApplySort(x => x.Name, filter.SortDesc)
        };

        return await Project(query).ToPagedResultAsync(filter, ct);
    }

    public async Task<StoreDto> GetByIdAsync(int id, CancellationToken ct = default)
        => await Project(_uow.Repository<Store>().Query().Where(x => x.Id == id)).FirstOrDefaultAsync(ct)
           ?? throw new AppException("Prodajni objekat nije pronadjen.", 404);

    public async Task<StoreDto> CreateAsync(SaveStoreRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Store>();
        var data = Validate(request);

        if (await repo.ExistsAsync(x => x.Code == data.Code, ct))
            throw new AppException("Prodajni objekat sa istom sifrom vec postoji.");

        if (await _uow.Repository<StorageLocation>().ExistsAsync(x => x.Code == data.Code + "-MAIN", ct))
            throw new AppException($"Lokacija {data.Code}-MAIN vec postoji. Izaberite drugu sifru objekta.");

        var store = new Store { IsActive = request.IsActive };
        Apply(store, data);
        var code = store.Code;

        await repo.AddAsync(store, ct);
        await _uow.SaveChangesAsync(ct);

        await _uow.Repository<StorageLocation>().AddAsync(new StorageLocation
        {
            Code = $"{code}-MAIN",
            Name = $"{store.Name} - prodajni prostor",
            LocationType = LocationType.Store,
            StoreId = store.Id,
            IsActive = true
        }, ct);

        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(store.Id, ct);
    }

    public async Task<StoreDto> UpdateAsync(int id, SaveStoreRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Store>();
        var data = Validate(request);

        var store = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Prodajni objekat nije pronadjen.", 404);

        if (await repo.ExistsAsync(x => x.Code == data.Code && x.Id != id, ct))
            throw new AppException("Prodajni objekat sa istom sifrom vec postoji.");

        Apply(store, data);
        store.IsActive = request.IsActive;

        repo.Update(store);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Store>();

        var store = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Prodajni objekat nije pronadjen.", 404);

        if (await _uow.Repository<Order>().ExistsAsync(x => x.StoreId == id, ct))
            throw new AppException("Objekat se ne moze obrisati jer postoje nalozi. Deaktivirajte ga umesto toga.");

        var hasStock = await _uow.Repository<StockItem>()
            .ExistsAsync(x => x.StorageLocation.StoreId == id && x.Quantity != 0, ct);

        if (hasStock)
            throw new AppException("Objekat se ne moze obrisati jer na njegovim lokacijama postoje zalihe.");

        var hasMovements = await _uow.Repository<StockMovement>().ExistsAsync(
            x => (x.FromLocation != null && x.FromLocation.StoreId == id)
              || (x.ToLocation != null && x.ToLocation.StoreId == id), ct);

        if (hasMovements)
            throw new AppException("Objekat se ne moze obrisati jer postoji istorija kretanja robe na njegovim lokacijama. Deaktivirajte ga umesto toga.");

        var locations = await _uow.Repository<StorageLocation>()
            .Query(asNoTracking: false)
            .Where(x => x.StoreId == id)
            .ToListAsync(ct);

        // Prazne stavke zaliha bez istorije ne smeju da blokiraju brisanje lokacija.
        var emptyStock = await _uow.Repository<StockItem>()
            .Query(asNoTracking: false)
            .Where(x => x.StorageLocation.StoreId == id)
            .ToListAsync(ct);

        foreach (var item in emptyStock)
            _uow.Repository<StockItem>().Remove(item);

        foreach (var location in locations)
            _uow.Repository<StorageLocation>().Remove(location);

        repo.Remove(store);
        await _uow.SaveChangesAsync(ct);
    }

    private static Store Validate(SaveStoreRequest request) => new()
    {
        Code = Guard.Code(request.Code, "Sifra", 15),
        Name = Guard.Required(request.Name, "Naziv", 200),
        ManagerName = Guard.Optional(request.ManagerName, "Odgovorno lice", 150),
        Email = Guard.Email(request.Email),
        Phone = Guard.Optional(request.Phone, "Telefon", 50),
        Address = Guard.Optional(request.Address, "Adresa", 300),
        City = Guard.Optional(request.City, "Grad", 100)
    };

    private static void Apply(Store target, Store data)
    {
        target.Code = data.Code;
        target.Name = data.Name;
        target.ManagerName = data.ManagerName;
        target.Email = data.Email;
        target.Phone = data.Phone;
        target.Address = data.Address;
        target.City = data.City;
    }

    private static IQueryable<StoreDto> Project(IQueryable<Store> query)
        => query.Select(x => new StoreDto(
            x.Id, x.Code, x.Name, x.ManagerName, x.Email, x.Phone, x.Address, x.City, x.IsActive,
            x.Orders.Count, x.StorageLocations.Count, x.CreatedAt));
}
