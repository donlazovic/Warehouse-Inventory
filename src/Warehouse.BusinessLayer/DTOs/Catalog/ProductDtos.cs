using Warehouse.BusinessLayer.Common;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.DTOs.Catalog;

public enum StockStatusFilter
{
    All = 0,
    OutOfStock = 1,
    BelowMinimum = 2,
    Normal = 3,
    AboveMaximum = 4
}

public class ProductFilterRequest : PagedRequest
{
    public int? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public StockStatusFilter? StockStatus { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public bool OnlyFavorites { get; set; }
}

public record ProductDto(
    int Id,
    string Sku,
    string Name,
    string? Description,
    UnitOfMeasure UnitOfMeasure,
    decimal Price,
    decimal MinStock,
    decimal MaxStock,
    bool IsActive,
    int CategoryId,
    string CategoryName,
    decimal TotalStock,
    bool IsFavorite,
    DateTime CreatedAt);

public record ProductStockByLocationDto(
    int StorageLocationId,
    string LocationCode,
    string LocationName,
    LocationType LocationType,
    string? StoreName,
    decimal Quantity,
    decimal EffectiveMinStock,
    bool IsBelowMinimum);

public record ProductDetailDto(
    ProductDto Product,
    IReadOnlyList<ProductStockByLocationDto> StockByLocation);

public record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    UnitOfMeasure UnitOfMeasure,
    decimal Price,
    decimal MinStock,
    decimal MaxStock,
    int CategoryId);

public record UpdateProductRequest(
    string Sku,
    string Name,
    string? Description,
    UnitOfMeasure UnitOfMeasure,
    decimal Price,
    decimal MinStock,
    decimal MaxStock,
    int CategoryId,
    bool IsActive);
