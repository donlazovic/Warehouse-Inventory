using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Inventory;

public class StorageLocationService : IStorageLocationService
{
    private readonly IUnitOfWork _uow;

    public StorageLocationService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<StorageLocationDto>> GetPagedAsync(StorageLocationFilterRequest filter, CancellationToken ct = default)
    {
        var query = _uow.Repository<StorageLocation>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
        }

        if (filter.LocationType.HasValue)
            query = query.Where(x => x.LocationType == filter.LocationType);

        if (filter.StoreId.HasValue)
            query = query.Where(x => x.StoreId == filter.StoreId);

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive);

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.ApplySort(x => x.Name, filter.SortDesc),
            "type" => query.ApplySort(x => x.LocationType, filter.SortDesc),
            _ => query.ApplySort(x => x.Code, filter.SortDesc)
        };

        return await Project(query).ToPagedResultAsync(filter, ct);
    }

    public Task<List<StorageLocationDto>> GetLookupAsync(CancellationToken ct = default)
        => Project(_uow.Repository<StorageLocation>().Query().Where(x => x.IsActive).OrderBy(x => x.Code))
            .ToListAsync(ct);

    public async Task<StorageLocationDto> GetByIdAsync(int id, CancellationToken ct = default)
        => await Project(_uow.Repository<StorageLocation>().Query().Where(x => x.Id == id)).FirstOrDefaultAsync(ct)
           ?? throw new AppException("Lokacija nije pronadjena.", 404);

    public async Task<StorageLocationDto> CreateAsync(SaveStorageLocationRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<StorageLocation>();
        var code = request.Code.Trim().ToUpperInvariant();

        if (await repo.ExistsAsync(x => x.Code == code, ct))
            throw new AppException("Lokacija sa istom sifrom vec postoji.");

        var storeId = await ValidateStoreAsync(request, ct);

        var location = new StorageLocation
        {
            Code = code,
            Name = request.Name.Trim(),
            Zone = request.Zone?.Trim(),
            LocationType = request.LocationType,
            StoreId = storeId,
            IsActive = request.IsActive
        };

        await repo.AddAsync(location, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(location.Id, ct);
    }

    public async Task<StorageLocationDto> UpdateAsync(int id, SaveStorageLocationRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<StorageLocation>();
        var code = request.Code.Trim().ToUpperInvariant();

        var location = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Lokacija nije pronadjena.", 404);

        if (await repo.ExistsAsync(x => x.Code == code && x.Id != id, ct))
            throw new AppException("Lokacija sa istom sifrom vec postoji.");

        var storeId = await ValidateStoreAsync(request, ct);

        location.Code = code;
        location.Name = request.Name.Trim();
        location.Zone = request.Zone?.Trim();
        location.LocationType = request.LocationType;
        location.StoreId = storeId;
        location.IsActive = request.IsActive;

        repo.Update(location);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<StorageLocation>();

        var location = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Lokacija nije pronadjena.", 404);

        if (await _uow.Repository<StockItem>().ExistsAsync(x => x.StorageLocationId == id && x.Quantity != 0, ct))
            throw new AppException("Lokacija se ne moze obrisati jer na njoj postoje zalihe.");

        var hasMovements = await _uow.Repository<StockMovement>()
            .ExistsAsync(x => x.FromLocationId == id || x.ToLocationId == id, ct);

        if (hasMovements)
            throw new AppException("Lokacija se ne moze obrisati jer postoji istorija kretanja robe. Deaktivirajte je umesto toga.");

        repo.Remove(location);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<int?> ValidateStoreAsync(SaveStorageLocationRequest request, CancellationToken ct)
    {
        if (request.LocationType == LocationType.Store)
        {
            var storeId = request.StoreId is > 0 ? request.StoreId : null;

            if (storeId is null)
                throw new AppException("Lokacija tipa prodajnog objekta mora biti vezana za objekat.");

            if (!await _uow.Repository<Store>().ExistsAsync(x => x.Id == storeId, ct))
                throw new AppException("Prodajni objekat ne postoji.");

            return storeId;
        }

        return null;
    }

    private static IQueryable<StorageLocationDto> Project(IQueryable<StorageLocation> query)
        => query.Select(x => new StorageLocationDto(
            x.Id, x.Code, x.Name, x.Zone, x.LocationType,
            x.StoreId, x.Store != null ? x.Store.Name : null,
            x.IsActive, x.StockItems.Count, x.CreatedAt));
}
