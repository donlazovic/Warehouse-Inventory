using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Partners;
using Warehouse.BusinessLayer.Services.Partners;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;

    public SuppliersController(ISupplierService service) => _service = service;

    [HttpGet]
    [HasPermission("suppliers.view")]
    public async Task<ActionResult<PagedResult<SupplierDto>>> GetPaged(
        [FromQuery] SupplierFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(filter, ct));

    [HttpGet("{id:int}")]
    [HasPermission("suppliers.view")]
    public async Task<ActionResult<SupplierDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [HasPermission("suppliers.create")]
    public async Task<ActionResult<SupplierDto>> Create(SaveSupplierRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [HasPermission("suppliers.update")]
    public async Task<ActionResult<SupplierDto>> Update(int id, SaveSupplierRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [HasPermission("suppliers.delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
