using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Catalog;

namespace Warehouse.BusinessLayer.Services.Catalog;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterRequest filter, int currentUserId, CancellationToken ct = default);
    Task<ProductDetailDto> GetByIdAsync(int id, int currentUserId, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, int currentUserId, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request, int currentUserId, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> ToggleFavoriteAsync(int productId, int currentUserId, CancellationToken ct = default);
}
