using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Catalog;

namespace Warehouse.BusinessLayer.Services.Catalog;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetPagedAsync(CategoryFilterRequest filter, CancellationToken ct = default);
    Task<List<CategoryTreeDto>> GetTreeAsync(CancellationToken ct = default);
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
