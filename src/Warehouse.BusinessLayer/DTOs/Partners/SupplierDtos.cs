using Warehouse.BusinessLayer.Common;

namespace Warehouse.BusinessLayer.DTOs.Partners;

public class SupplierFilterRequest : PagedRequest
{
    public bool? IsActive { get; set; }
    public string? City { get; set; }
}

public record SupplierDto(
    int Id,
    string Name,
    string? TaxNumber,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    bool IsActive,
    int OrderCount,
    decimal TotalPurchaseValue,
    DateTime? LastOrderDate,
    DateTime CreatedAt);

public record SaveSupplierRequest(
    string Name,
    string? TaxNumber,
    string? ContactPerson,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    bool IsActive);
