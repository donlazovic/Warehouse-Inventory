using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.BusinessLayer.Services.Inventory;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/storage-locations")]
public class StorageLocationsController : ControllerBase
{
    private readonly IStorageLocationService _service;

    public StorageLocationsController(IStorageLocationService service) => _service = service;

    [HttpGet]
    [HasPermission("stock.view")]
    public async Task<ActionResult<PagedResult<StorageLocationDto>>> GetPaged(
        [FromQuery] StorageLocationFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(filter, ct));

    [HttpGet("lookup")]
    [HasPermission("stock.view")]
    public async Task<ActionResult<List<StorageLocationDto>>> GetLookup(CancellationToken ct)
        => Ok(await _service.GetLookupAsync(ct));

    [HttpGet("{id:int}")]
    [HasPermission("stock.view")]
    public async Task<ActionResult<StorageLocationDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [HasPermission("stock.update")]
    public async Task<ActionResult<StorageLocationDto>> Create(SaveStorageLocationRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [HasPermission("stock.update")]
    public async Task<ActionResult<StorageLocationDto>> Update(int id, SaveStorageLocationRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [HasPermission("stock.update")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
