using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Auth;
using Warehouse.BusinessLayer.Services.Email;
using Warehouse.BusinessLayer.Settings;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Identity;

namespace Warehouse.BusinessLayer.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _settings;
    private readonly IEmailSender _email;
    private readonly ILogger<AuthService> _logger;

    private const int MinPasswordLength = 8;

    public AuthService(
        IUnitOfWork uow,
        IJwtTokenService tokenService,
        IOptions<JwtSettings> settings,
        IEmailSender email,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _tokenService = tokenService;
        _settings = settings.Value;
        _email = email;
        _logger = logger;
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

        if (user.ApprovedAt is null)
            throw new AppException("Nalog jos nije odobren od strane administratora.", 403);

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

        if (stored.User.ApprovedAt is null || !stored.User.IsActive)
            throw new AppException("Nalog vise nije aktivan.", 403);

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

    public async Task<CurrentUserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;

        if (firstName.Length == 0 || lastName.Length == 0)
            throw new AppException("Ime i prezime su obavezni.");

        if (firstName.Length > 100 || lastName.Length > 100)
            throw new AppException("Ime i prezime mogu imati najvise 100 karaktera.");

        var repo = _uow.Repository<User>();
        var user = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new AppException("Korisnik nije pronadjen.", 404);

        user.FirstName = firstName;
        user.LastName = lastName;
        repo.Update(user);
        await _uow.SaveChangesAsync(ct);

        return await GetCurrentUserAsync(userId, ct);
    }

    public async Task<AuthResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request, string? ip, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>()
            .Query(asNoTracking: false)
            .Include(x => x.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == userId, ct)
            ?? throw new AppException("Korisnik nije pronadjen.", 404);

        if (string.IsNullOrEmpty(request.CurrentPassword) || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new AppException("Trenutna lozinka nije tacna.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < MinPasswordLength)
            throw new AppException($"Nova lozinka mora imati najmanje {MinPasswordLength} karaktera.");

        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            throw new AppException("Nova lozinka mora se razlikovati od trenutne.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        var now = DateTime.UtcNow;
        var sessions = await _uow.Repository<RefreshToken>()
            .Query(asNoTracking: false)
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.RevokedAt = now;
            _uow.Repository<RefreshToken>().Update(session);
        }

        var response = await IssueTokensAsync(user, ip, ct);

        try
        {
            var (html, text) = EmailTemplates.PasswordChanged(user.FirstName, now.ToLocalTime());
            await _email.SendAsync(new EmailMessage(user.Email, user.FirstName, "Lozinka je promenjena — Skladišnik", html, text), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Potvrda o promeni lozinke korisniku {UserId} nije poslata.", userId);
        }

        return response;
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
        => new(user.Id, user.FirstName, user.LastName, user.Email, user.Role.Name, permissions, user.IsOwner);
}
