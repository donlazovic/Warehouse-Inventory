using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.BusinessLayer.DTOs.Reports;
using Warehouse.BusinessLayer.Services.Reports;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    [HttpGet("dashboard")]
    [HasPermission("reports.view")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(CancellationToken ct)
        => Ok(await _service.GetDashboardAsync(ct));

    [HttpGet("turnover")]
    [HasPermission("reports.view")]
    public async Task<ActionResult<TurnoverReportDto>> GetTurnover(
        [FromQuery] TurnoverRequest request, CancellationToken ct)
        => Ok(await _service.GetTurnoverAsync(request, ct));

    [HttpGet("snapshot")]
    [HasPermission("reports.view")]
    public async Task<ActionResult<SnapshotReportDto>> GetSnapshot(
        [FromQuery] SnapshotRequest request, CancellationToken ct)
        => Ok(await _service.GetSnapshotAsync(request, ct));

    [HttpGet("supplier-activity/{supplierId:int}")]
    [HasPermission("reports.view")]
    public async Task<ActionResult<SupplierActivityDto>> GetSupplierActivity(
        int supplierId, [FromQuery] SupplierActivityRequest request, CancellationToken ct)
        => Ok(await _service.GetSupplierActivityAsync(supplierId, request, ct));
}
