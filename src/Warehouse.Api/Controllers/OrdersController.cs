using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.Api.Extensions;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Orders;
using Warehouse.BusinessLayer.Services.Orders;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service) => _service = service;

    [HttpGet]
    [HasPermission("orders.view")]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetPaged(
        [FromQuery] OrderFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(filter, ct));

    [HttpGet("kanban")]
    [HasPermission("orders.view")]
    public async Task<ActionResult<List<KanbanColumnDto>>> GetKanban(
        [FromQuery] OrderFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetKanbanAsync(filter, ct));

    [HttpGet("{id:int}")]
    [HasPermission("orders.view")]
    public async Task<ActionResult<OrderDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [HasPermission("orders.create")]
    public async Task<ActionResult<OrderDetailDto>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Order.Id }, created);
    }

    [HttpPut("{id:int}")]
    [HasPermission("orders.update")]
    public async Task<ActionResult<OrderDetailDto>> Update(int id, UpdateOrderRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [HasPermission("orders.delete")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<ActionResult<OrderDetailDto>> ChangeStatus(
        int id, ChangeOrderStatusRequest request, CancellationToken ct)
        => Ok(await _service.ChangeStatusAsync(id, request, User.GetUserId(), User.HasPermission, ct));
}
