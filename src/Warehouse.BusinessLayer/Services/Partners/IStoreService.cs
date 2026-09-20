using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Partners;

namespace Warehouse.BusinessLayer.Services.Partners;

public interface IStoreService
{
    Task<PagedResult<StoreDto>> GetPagedAsync(StoreFilterRequest filter, CancellationToken ct = default);
    Task<StoreDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<StoreDto> CreateAsync(SaveStoreRequest request, CancellationToken ct = default);
    Task<StoreDto> UpdateAsync(int id, SaveStoreRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
