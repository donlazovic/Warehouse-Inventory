using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Inventory;

namespace Warehouse.BusinessLayer.Services.Inventory;

public interface IStorageLocationService
{
    Task<PagedResult<StorageLocationDto>> GetPagedAsync(StorageLocationFilterRequest filter, CancellationToken ct = default);
    Task<List<StorageLocationDto>> GetLookupAsync(CancellationToken ct = default);
    Task<StorageLocationDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StorageLocationDto> CreateAsync(SaveStorageLocationRequest request, CancellationToken ct = default);
    Task<StorageLocationDto> UpdateAsync(int id, SaveStorageLocationRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
