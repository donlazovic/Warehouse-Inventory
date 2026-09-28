using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Warehouse.Api.Authorization;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.BusinessLayer.DTOs.Orders;
using Warehouse.BusinessLayer.DTOs.Reports;
using Warehouse.BusinessLayer.Services.Export;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/exports")]
[EnableRateLimiting("export")]
public class ExportsController : ControllerBase
{
    private readonly IExportService _service;

    public ExportsController(IExportService service) => _service = service;

    [HttpGet("orders/{id:int}/document")]
    [HasPermission("orders.view")]
    public async Task<IActionResult> OrderDocument(int id, CancellationToken ct)
        => ToFile(await _service.OrderDocumentAsync(id, ct));

    [HttpGet("orders")]
    [HasPermission("orders.export")]
    public async Task<IActionResult> Orders(
        [FromQuery] OrderFilterRequest filter, [FromQuery] ExportFormat format, CancellationToken ct)
        => ToFile(await _service.OrdersAsync(filter, format, ct));

    [HttpGet("stock")]
    [HasPermission("stock.export")]
    public async Task<IActionResult> Stock(
        [FromQuery] StockFilterRequest filter, [FromQuery] ExportFormat format, CancellationToken ct)
        => ToFile(await _service.StockAsync(filter, format, ct));

    [HttpGet("movements")]
    [HasPermission("stock.export")]
    public async Task<IActionResult> Movements(
        [FromQuery] StockMovementFilterRequest filter, [FromQuery] ExportFormat format, CancellationToken ct)
        => ToFile(await _service.MovementsAsync(filter, format, ct));

    [HttpGet("reports/turnover")]
    [HasPermission("reports.export")]
    public async Task<IActionResult> Turnover(
        [FromQuery] TurnoverRequest request, [FromQuery] ExportFormat format, CancellationToken ct)
        => ToFile(await _service.TurnoverAsync(request, format, ct));

    [HttpGet("reports/snapshot")]
    [HasPermission("reports.export")]
    public async Task<IActionResult> Snapshot(
        [FromQuery] SnapshotRequest request, [FromQuery] ExportFormat format, CancellationToken ct)
        => ToFile(await _service.SnapshotAsync(request, format, ct));

    [HttpGet("reports/supplier-activity/{supplierId:int}")]
    [HasPermission("reports.export")]
    public async Task<IActionResult> SupplierActivity(
        int supplierId, [FromQuery] SupplierActivityRequest request, [FromQuery] ExportFormat format, CancellationToken ct)
        => ToFile(await _service.SupplierActivityAsync(supplierId, request, format, ct));

    private FileContentResult ToFile(ExportFile file) => File(file.Content, file.ContentType, file.FileName);
}
