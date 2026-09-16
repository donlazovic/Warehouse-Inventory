using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Catalog;
using Warehouse.BusinessLayer.Services.Catalog;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    [HasPermission("categories.view")]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetPaged(
        [FromQuery] CategoryFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(filter, ct));

    [HttpGet("tree")]
    [HasPermission("categories.view")]
    public async Task<ActionResult<List<CategoryTreeDto>>> GetTree(CancellationToken ct)
        => Ok(await _service.GetTreeAsync(ct));

    [HttpGet("{id:int}")]
    [HasPermission("categories.view")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [HasPermission("categories.create")]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [HasPermission("categories.update")]
    public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [HasPermission("categories.delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
