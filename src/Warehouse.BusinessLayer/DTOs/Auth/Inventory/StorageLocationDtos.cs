using Warehouse.BusinessLayer.Common;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.DTOs.Inventory;

public class StorageLocationFilterRequest : PagedRequest
{
    public LocationType? LocationType { get; set; }
    public int? StoreId { get; set; }
    public bool? IsActive { get; set; }
}

public record StorageLocationDto(
    int Id,
    string Code,
    string Name,
    string? Zone,
    LocationType LocationType,
    int? StoreId,
    string? StoreName,
    bool IsActive,
    int ProductCount,
    DateTime CreatedAt);

public record SaveStorageLocationRequest(
    string Code,
    string Name,
    string? Zone,
    LocationType LocationType,
    int? StoreId,
    bool IsActive);
