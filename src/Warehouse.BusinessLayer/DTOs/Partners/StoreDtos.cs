using Warehouse.BusinessLayer.Common;

namespace Warehouse.BusinessLayer.DTOs.Partners;

public class StoreFilterRequest : PagedRequest
{
    public bool? IsActive { get; set; }
    public string? City { get; set; }
}

public record StoreDto(
    int Id,
    string Code,
    string Name,
    string? ManagerName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    bool IsActive,
    int OrderCount,
    int LocationCount,
    DateTime CreatedAt);

public record SaveStoreRequest(
    string Code,
    string Name,
    string? ManagerName,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    bool IsActive);
