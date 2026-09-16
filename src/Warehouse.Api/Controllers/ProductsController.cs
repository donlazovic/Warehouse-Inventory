using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.Api.Extensions;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Catalog;
using Warehouse.BusinessLayer.Services.Catalog;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    [HasPermission("products.view")]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged(
        [FromQuery] ProductFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(filter, User.GetUserId(), ct));

    [HttpGet("{id:int}")]
    [HasPermission("products.view")]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, User.GetUserId(), ct));

    [HttpPost]
    [HasPermission("products.create")]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [HasPermission("products.update")]
    public async Task<ActionResult<ProductDto>> Update(int id, UpdateProductRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, User.GetUserId(), ct));

    [HttpDelete("{id:int}")]
    [HasPermission("products.delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/favorite")]
    [HasPermission("products.view")]
    public async Task<ActionResult<object>> ToggleFavorite(int id, CancellationToken ct)
    {
        var isFavorite = await _service.ToggleFavoriteAsync(id, User.GetUserId(), ct);
        return Ok(new { isFavorite });
    }
}
