using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.DTOs.Reports;

public record KpiDto(
    decimal StockValue,
    int OpenOrders,
    int PendingApproval,
    int BelowMinimum,
    int CompletedThisMonth,
    int CompletedLastMonth,
    decimal IssuedValueThisMonth,
    decimal IssuedValueLastMonth);

public record DailyFlowDto(DateTime Date, decimal ReceivedValue, decimal TransferredValue, decimal IssuedValue);

public record CategoryValueDto(string Category, decimal Value);

public record StatusCountDto(OrderStatus Status, int Count);

public record TopProductDto(
    int ProductId,
    string Sku,
    string Name,
    UnitOfMeasure UnitOfMeasure,
    decimal Quantity,
    decimal Value);

public record LowStockDto(
    int StockItemId,
    string ProductName,
    string LocationCode,
    string? StoreName,
    UnitOfMeasure UnitOfMeasure,
    decimal Quantity,
    decimal MinStock);

public record DashboardDto(
    KpiDto Kpi,
    IReadOnlyList<DailyFlowDto> DailyFlow,
    IReadOnlyList<CategoryValueDto> StockByCategory,
    IReadOnlyList<TopProductDto> TopIssued,
    IReadOnlyList<LowStockDto> LowStock,
    IReadOnlyList<StatusCountDto> OrdersByStatus);

public class TurnoverRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int? LocationId { get; set; }
    public int? CategoryId { get; set; }
}

public record TurnoverRowDto(
    int ProductId,
    string Sku,
    string Name,
    string Category,
    UnitOfMeasure UnitOfMeasure,
    decimal Opening,
    decimal Received,
    decimal TransferredIn,
    decimal TransferredOut,
    decimal Issued,
    decimal AdjustmentNet,
    decimal Closing,
    decimal ReceivedValue,
    decimal IssuedValue);

public record TurnoverReportDto(
    DateTime From,
    DateTime To,
    int? LocationId,
    decimal TotalReceivedValue,
    decimal TotalIssuedValue,
    IReadOnlyList<TurnoverRowDto> Rows);

public class SnapshotRequest
{
    public DateTime? At { get; set; }
    public int? LocationId { get; set; }
    public int? CategoryId { get; set; }
}

public record SnapshotRowDto(
    int ProductId,
    string Sku,
    string Name,
    string Category,
    UnitOfMeasure UnitOfMeasure,
    int LocationId,
    string LocationCode,
    string LocationName,
    decimal Quantity,
    decimal Price,
    decimal Value);

public record SnapshotReportDto(
    DateTime At,
    decimal TotalValue,
    IReadOnlyList<SnapshotRowDto> Rows);

public class SupplierActivityRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public record MonthValueDto(int Year, int Month, int Orders, decimal Value);

public record SupplierActivityDto(
    int SupplierId,
    string SupplierName,
    DateTime From,
    DateTime To,
    int TotalOrders,
    int CompletedOrders,
    int CancelledOrders,
    int OpenOrders,
    decimal CompletedValue,
    double? AverageLeadDays,
    IReadOnlyList<MonthValueDto> Monthly,
    IReadOnlyList<TopProductDto> TopProducts);
