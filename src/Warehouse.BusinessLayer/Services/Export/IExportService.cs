using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.BusinessLayer.DTOs.Orders;
using Warehouse.BusinessLayer.DTOs.Reports;

namespace Warehouse.BusinessLayer.Services.Export;

public interface IExportService
{
    Task<ExportFile> OrderDocumentAsync(int orderId, CancellationToken ct = default);
    Task<ExportFile> OrdersAsync(OrderFilterRequest filter, ExportFormat format, CancellationToken ct = default);
    Task<ExportFile> StockAsync(StockFilterRequest filter, ExportFormat format, CancellationToken ct = default);
    Task<ExportFile> MovementsAsync(StockMovementFilterRequest filter, ExportFormat format, CancellationToken ct = default);
    Task<ExportFile> TurnoverAsync(TurnoverRequest request, ExportFormat format, CancellationToken ct = default);
    Task<ExportFile> SnapshotAsync(SnapshotRequest request, ExportFormat format, CancellationToken ct = default);
    Task<ExportFile> SupplierActivityAsync(int supplierId, SupplierActivityRequest request, ExportFormat format, CancellationToken ct = default);
}
