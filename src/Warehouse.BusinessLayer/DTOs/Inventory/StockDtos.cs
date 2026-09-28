using Warehouse.BusinessLayer.Common;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.DTOs.Inventory;

public class StockFilterRequest : PagedRequest
{
    public int? ProductId { get; set; }
    public int? StorageLocationId { get; set; }
    public int? StoreId { get; set; }
    public int? CategoryId { get; set; }
    public LocationType? LocationType { get; set; }
    public bool OnlyBelowMinimum { get; set; }
    public bool OnlyInStock { get; set; }
}

public record StockItemDto(
    int Id,
    int ProductId,
    string ProductSku,
    string ProductName,
    UnitOfMeasure UnitOfMeasure,
    string CategoryName,
    int StorageLocationId,
    string LocationCode,
    string LocationName,
    LocationType LocationType,
    int? StoreId,
    string? StoreName,
    decimal Quantity,
    decimal EffectiveMinStock,
    decimal EffectiveMaxStock,
    decimal? MinStockOverride,
    decimal? MaxStockOverride,
    bool IsBelowMinimum,
    DateTime? UpdatedAt);

public class StockMovementFilterRequest : PagedRequest
{
    public int? ProductId { get; set; }
    public int? LocationId { get; set; }
    public MovementType? MovementType { get; set; }
    public IssueReason? IssueReason { get; set; }
    public int? OrderId { get; set; }
    public int? UserId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public record StockMovementDto(
    int Id,
    MovementType MovementType,
    IssueReason? IssueReason,
    int ProductId,
    string ProductSku,
    string ProductName,
    UnitOfMeasure UnitOfMeasure,
    decimal Quantity,
    int? FromLocationId,
    string? FromLocationName,
    int? ToLocationId,
    string? ToLocationName,
    int? OrderId,
    string? OrderNumber,
    string UserName,
    string? Note,
    DateTime CreatedAt);

public record AdjustStockRequest(
    int ProductId,
    int StorageLocationId,
    decimal NewQuantity,
    string? Note);

public record SetStockLimitsRequest(
    decimal? MinStockOverride,
    decimal? MaxStockOverride);

public record IssueStockLineRequest(int ProductId, decimal Quantity);

public record IssueStockRequest(
    int StorageLocationId,
    IssueReason Reason,
    string? Note,
    List<IssueStockLineRequest> Items);

public record StockMismatchDto(
    int ProductId,
    string ProductName,
    int LocationId,
    string LocationName,
    decimal RecordedQuantity,
    decimal ExpectedQuantity);

public record StockReconciliationDto(
    DateTime CheckedAt,
    int CheckedCount,
    bool IsConsistent,
    IReadOnlyList<StockMismatchDto> Mismatches);
