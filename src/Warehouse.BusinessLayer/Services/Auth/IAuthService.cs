using Warehouse.BusinessLayer.DTOs.Auth;

namespace Warehouse.BusinessLayer.Services.Auth;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ip, CancellationToken ct = default);
    Task<AuthResponse> RefreshAsync(RefreshRequest request, string? ip, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
    Task<CurrentUserDto> GetCurrentUserAsync(int userId, CancellationToken ct = default);
}
