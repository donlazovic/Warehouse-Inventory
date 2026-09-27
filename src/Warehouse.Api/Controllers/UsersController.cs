using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Warehouse.Api.Authorization;
using Warehouse.Api.Extensions;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Identity;
using Warehouse.BusinessLayer.Services.Identity;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        await _service.RegisterAsync(request, ct);
        return Accepted(new
        {
            message = "Zahtev je poslat. Administrator ce odobriti nalog pre prve prijave."
        });
    }

    [HttpGet]
    [HasPermission("users.view")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetPaged(
        [FromQuery] UserFilterRequest filter, CancellationToken ct)
        => Ok(await _service.GetPagedAsync(filter, ct));

    [HttpGet("pending-count")]
    [HasPermission("users.view")]
    public async Task<ActionResult<object>> GetPendingCount(CancellationToken ct)
        => Ok(new { count = await _service.GetPendingCountAsync(ct) });

    [HttpGet("{id:int}")]
    [HasPermission("users.view")]
    public async Task<ActionResult<UserDto>> GetById(int id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [HasPermission("users.create")]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, User.GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [HasPermission("users.update")]
    public async Task<ActionResult<UserDto>> Update(int id, UpdateUserRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, User.GetUserId(), ct));

    [HttpPost("{id:int}/approve")]
    [HasPermission("users.update")]
    public async Task<ActionResult<UserDto>> Approve(int id, ApproveUserRequest request, CancellationToken ct)
        => Ok(await _service.ApproveAsync(id, request, User.GetUserId(), ct));

    [HttpDelete("{id:int}/reject")]
    [HasPermission("users.delete")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        await _service.RejectAsync(id, ct);
        return NoContent();
    }

    [HttpPut("{id:int}/password")]
    [HasPermission("users.update")]
    public async Task<IActionResult> ResetPassword(int id, ResetPasswordRequest request, CancellationToken ct)
    {
        await _service.ResetPasswordAsync(id, request, ct);
        return NoContent();
    }
}
