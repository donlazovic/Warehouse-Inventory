using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Authorization;
using Warehouse.BusinessLayer.DTOs.Identity;
using Warehouse.BusinessLayer.Services.Identity;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service) => _service = service;

    [HttpGet]
    [HasPermission("users.view")]
    public async Task<ActionResult<List<RoleDto>>> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllAsync(ct));

    [HttpGet("permission-tree")]
    [HasPermission("users.view")]
    public async Task<ActionResult<List<PermissionNodeDto>>> GetPermissionTree(CancellationToken ct)
        => Ok(await _service.GetPermissionTreeAsync(ct));

    [HttpGet("{id:int}")]
    [HasPermission("users.view")]
    public async Task<ActionResult<RoleDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPut("{id:int}")]
    [HasPermission("users.update")]
    public async Task<ActionResult<RoleDetailDto>> Update(int id, UpdateRoleRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));
}
