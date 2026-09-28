using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.Api.Extensions;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.BusinessLayer.Services.Inventory;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/stock")]
public class StockController : ControllerBase
{
    private readonly IStockService _service;

    public StockController(IStockService service) => _service = service;

    [HttpGet]
    [HasPermission("stock.view")]
    public async Task<ActionResult<PagedResult<StockItemDto>>> GetStock(
        [FromQuery] StockFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetStockAsync(filter, ct));

    [HttpGet("movements")]
    [HasPermission("stock.view")]
    public async Task<ActionResult<PagedResult<StockMovementDto>>> GetMovements(
        [FromQuery] StockMovementFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetMovementsAsync(filter, ct));

    [HttpGet("reconciliation")]
    [HasPermission("stock.view")]
    public async Task<ActionResult<StockReconciliationDto>> GetReconciliation(CancellationToken ct)
        => Ok(await _service.GetReconciliationAsync(ct));

    [HttpPost("adjust")]
    [HasPermission("stock.update")]
    public async Task<ActionResult<StockItemDto>> Adjust(AdjustStockRequest request, CancellationToken ct)
        => Ok(await _service.AdjustAsync(request, User.GetUserId(), ct));

    [HttpPost("issue")]
    [HasPermission("stock.issue")]
    public async Task<ActionResult<object>> Issue(IssueStockRequest request, CancellationToken ct)
    {
        var lines = await _service.IssueAsync(request, User.GetUserId(), ct);
        return Ok(new { issuedLines = lines });
    }

    [HttpPut("{stockItemId:int}/limits")]
    [HasPermission("stock.update")]
    public async Task<ActionResult<StockItemDto>> SetLimits(
        int stockItemId, SetStockLimitsRequest request, CancellationToken ct)
        => Ok(await _service.SetLimitsAsync(stockItemId, request, ct));
}
