using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Auth;
using Warehouse.BusinessLayer.Settings;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Identity;

namespace Warehouse.BusinessLayer.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _settings;

    public AuthService(IUnitOfWork uow, IJwtTokenService tokenService, IOptions<JwtSettings> settings)
    {
        _uow = uow;
        _tokenService = tokenService;
        _settings = settings.Value;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ip, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>()
            .Query()
            .Include(x => x.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Email == request.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new AppException("Pogresan email ili lozinka.", 401);

        if (!user.IsActive)
            throw new AppException("Korisnicki nalog je deaktiviran.", 403);

        return await IssueTokensAsync(user, ip, ct);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, string? ip, CancellationToken ct = default)
    {
        var tokenRepo = _uow.Repository<RefreshToken>();

        var stored = await tokenRepo
            .Query(asNoTracking: false)
            .Include(x => x.User)
                .ThenInclude(u => u.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, ct);

        if (stored is null || !stored.IsActive)
            throw new AppException("Refresh token nije validan.", 401);

        stored.RevokedAt = DateTime.UtcNow;
        tokenRepo.Update(stored);

        return await IssueTokensAsync(stored.User, ip, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenRepo = _uow.Repository<RefreshToken>();

        var stored = await tokenRepo
            .Query(asNoTracking: false)
            .FirstOrDefaultAsync(x => x.Token == refreshToken, ct);

        if (stored is null || !stored.IsActive)
            return;

        stored.RevokedAt = DateTime.UtcNow;
        tokenRepo.Update(stored);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(int userId, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>()
            .Query()
            .Include(x => x.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        if (user is null)
            throw new AppException("Korisnik nije pronadjen.", 404);

        return MapCurrentUser(user, GetPermissions(user));
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, string? ip, CancellationToken ct)
    {
        var permissions = GetPermissions(user);
        var (accessToken, expiresAt) = _tokenService.CreateAccessToken(user, permissions);

        var refreshToken = new RefreshToken
        {
            Token = _tokenService.CreateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenDays),
            CreatedByIp = ip,
            UserId = user.Id
        };

        await _uow.Repository<RefreshToken>().AddAsync(refreshToken, ct);

        user.LastLoginAt = DateTime.UtcNow;
        _uow.Repository<User>().Update(user);

        await _uow.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, refreshToken.Token, expiresAt, MapCurrentUser(user, permissions));
    }

    private static List<string> GetPermissions(User user)
        => user.Role.RolePermissions
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

    private static CurrentUserDto MapCurrentUser(User user, IReadOnlyList<string> permissions)
        => new(user.Id, user.FirstName, user.LastName, user.Email, user.Role.Name, permissions);
}
