using Warehouse.BusinessLayer.Common;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.DTOs.Orders;

public class OrderFilterRequest : PagedRequest
{
    public OrderType? OrderType { get; set; }
    public OrderStatus? Status { get; set; }
    public int? SupplierId { get; set; }
    public int? StoreId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}

public record OrderItemDto(
    int Id,
    int ProductId,
    string ProductSku,
    string ProductName,
    UnitOfMeasure UnitOfMeasure,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record OrderDto(
    int Id,
    string OrderNumber,
    OrderType OrderType,
    OrderStatus Status,
    decimal TotalValue,
    string? Note,
    int? SupplierId,
    string? SupplierName,
    int? StoreId,
    string? StoreName,
    int? SourceLocationId,
    string? SourceLocationName,
    int? DestinationLocationId,
    string? DestinationLocationName,
    string CreatedByName,
    string? ApprovedByName,
    DateTime? ApprovedAt,
    DateTime? CompletedAt,
    int ItemCount,
    DateTime CreatedAt);

public record OrderStatusHistoryDto(
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    string ChangedByName,
    string? Note,
    DateTime ChangedAt);

public record OrderDetailDto(
    OrderDto Order,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> History,
    IReadOnlyList<OrderStatus> AllowedNextStatuses);

public record SaveOrderItemRequest(int ProductId, decimal Quantity, decimal? UnitPrice);

public record CreateOrderRequest(
    OrderType OrderType,
    int? SupplierId,
    int? StoreId,
    int? SourceLocationId,
    int? DestinationLocationId,
    string? Note,
    List<SaveOrderItemRequest> Items);

public record UpdateOrderRequest(
    int? SupplierId,
    int? StoreId,
    int? SourceLocationId,
    int? DestinationLocationId,
    string? Note,
    List<SaveOrderItemRequest> Items);

public record ChangeOrderStatusRequest(OrderStatus Status, string? Note);

public record KanbanColumnDto(
    OrderStatus Status,
    string Title,
    int TotalCount,
    decimal TotalValue,
    IReadOnlyList<OrderDto> Orders);
