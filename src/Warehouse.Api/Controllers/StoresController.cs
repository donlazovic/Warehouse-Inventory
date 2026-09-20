using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Partners;
using Warehouse.BusinessLayer.Services.Partners;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/stores")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _service;

    public StoresController(IStoreService service) => _service = service;

    [HttpGet]
    [HasPermission("stores.view")]
    public async Task<ActionResult<PagedResult<StoreDto>>> GetPaged(
        [FromQuery] StoreFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(filter, ct));

    [HttpGet("{id:int}")]
    [HasPermission("stores.view")]
    public async Task<ActionResult<StoreDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [HasPermission("stores.create")]
    public async Task<ActionResult<StoreDto>> Create(SaveStoreRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [HasPermission("stores.update")]
    public async Task<ActionResult<StoreDto>> Update(int id, SaveStoreRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [HasPermission("stores.delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
