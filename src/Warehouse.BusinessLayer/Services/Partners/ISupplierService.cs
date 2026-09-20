using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Partners;

namespace Warehouse.BusinessLayer.Services.Partners;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetPagedAsync(SupplierFilterRequest filter, CancellationToken ct = default);
    Task<SupplierDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SupplierDto> CreateAsync(SaveSupplierRequest request, CancellationToken ct = default);
    Task<SupplierDto> UpdateAsync(int id, SaveSupplierRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
