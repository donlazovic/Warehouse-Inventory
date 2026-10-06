using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Warehouse.BusinessLayer.DTOs.Auth;
using Warehouse.BusinessLayer.Services.Auth;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting("auth")]
public class PasswordResetController : ControllerBase
{
    private readonly IPasswordResetService _service;

    public PasswordResetController(IPasswordResetService service) => _service = service;

    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot(ForgotPasswordRequest request, CancellationToken ct)
    {
        await _service.RequestAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Accepted(new
        {
            message = "Ako nalog sa tom adresom postoji, poslali smo link za promenu lozinke."
        });
    }

    [HttpGet("reset-password/check")]
    public async Task<ActionResult<ResetTokenStatusDto>> Check([FromQuery] string token, CancellationToken ct)
        => Ok(await _service.CheckAsync(token, ct));

    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordWithTokenRequest request, CancellationToken ct)
    {
        await _service.ResetAsync(request, ct);
        return NoContent();
    }
}
