using Warehouse.BusinessLayer.DTOs.Auth;

namespace Warehouse.BusinessLayer.Services.Auth;

public interface IPasswordResetService
{
    Task RequestAsync(ForgotPasswordRequest request, string? ip, CancellationToken ct = default);
    Task<ResetTokenStatusDto> CheckAsync(string token, CancellationToken ct = default);
    Task ResetAsync(ResetPasswordWithTokenRequest request, CancellationToken ct = default);
}
