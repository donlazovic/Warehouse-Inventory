using Warehouse.BusinessLayer.DTOs.Reports;

namespace Warehouse.BusinessLayer.Services.Reports;

public interface IReportService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<TurnoverReportDto> GetTurnoverAsync(TurnoverRequest request, CancellationToken ct = default);
    Task<SnapshotReportDto> GetSnapshotAsync(SnapshotRequest request, CancellationToken ct = default);
    Task<SupplierActivityDto> GetSupplierActivityAsync(int supplierId, SupplierActivityRequest request, CancellationToken ct = default);
}
