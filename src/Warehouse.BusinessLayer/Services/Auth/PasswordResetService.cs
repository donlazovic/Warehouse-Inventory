using System.Security.Cryptography;
using System.Text;
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

public class PasswordResetService : IPasswordResetService
{
    private const int TokenLifetimeMinutes = 30;
    private const int RequestCooldownSeconds = 60;
    private const int MinPasswordLength = 8;
    private const string InvalidLinkMessage = "Link za promenu lozinke nije validan ili je istekao. Zatrazite novi.";

    private readonly IUnitOfWork _uow;
    private readonly IEmailSender _email;
    private readonly ClientSettings _client;
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(
        IUnitOfWork uow,
        IEmailSender email,
        IOptions<ClientSettings> client,
        ILogger<PasswordResetService> logger)
    {
        _uow = uow;
        _email = email;
        _client = client.Value;
        _logger = logger;
    }

    public async Task RequestAsync(ForgotPasswordRequest request, string? ip, CancellationToken ct = default)
    {
        var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (email.Length == 0)
            return;

        var user = await _uow.Repository<User>()
            .Query()
            .Where(x => x.Email == email && x.IsActive && x.ApprovedAt != null)
            .Select(x => new { x.Id, x.FirstName, x.Email })
            .FirstOrDefaultAsync(ct);

        if (user is null)
            return;

        var now = DateTime.UtcNow;
        var tokens = _uow.Repository<PasswordResetToken>();

        var recentlyRequested = await tokens.ExistsAsync(
            x => x.UserId == user.Id && x.CreatedAt > now.AddSeconds(-RequestCooldownSeconds), ct);

        if (recentlyRequested)
            return;

        var stillValid = await tokens.Query(asNoTracking: false)
            .Where(x => x.UserId == user.Id && x.UsedAt == null && x.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var old in stillValid)
        {
            old.ExpiresAt = now;
            tokens.Update(old);
        }

        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        await tokens.AddAsync(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = Hash(rawToken),
            ExpiresAt = now.AddMinutes(TokenLifetimeMinutes),
            RequestedByIp = ip
        }, ct);

        await _uow.SaveChangesAsync(ct);

        var link = $"{_client.BaseUrl.TrimEnd('/')}/reset-lozinke?token={rawToken}";
        var (html, text) = EmailTemplates.PasswordReset(user.FirstName, link, TokenLifetimeMinutes);

        try
        {
            await _email.SendAsync(new EmailMessage(user.Email, user.FirstName, "Promena lozinke — Skladišnik", html, text), ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Slanje mejla za promenu lozinke korisniku {UserId} nije uspelo.", user.Id);
        }
    }

    public async Task<ResetTokenStatusDto> CheckAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return new ResetTokenStatusDto(false, null);

        var hash = Hash(token);
        var now = DateTime.UtcNow;

        var email = await _uow.Repository<PasswordResetToken>()
            .Query()
            .Where(x => x.TokenHash == hash && x.UsedAt == null && x.ExpiresAt > now
                     && x.User.IsActive && x.User.ApprovedAt != null)
            .Select(x => x.User.Email)
            .FirstOrDefaultAsync(ct);

        return new ResetTokenStatusDto(email is not null, email is null ? null : MaskEmail(email));
    }

    public async Task ResetAsync(ResetPasswordWithTokenRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < MinPasswordLength)
            throw new AppException($"Lozinka mora imati najmanje {MinPasswordLength} karaktera.");

        if (string.IsNullOrWhiteSpace(request.Token))
            throw new AppException(InvalidLinkMessage);

        var hash = Hash(request.Token);
        var now = DateTime.UtcNow;

        var record = await _uow.Repository<PasswordResetToken>()
            .Query(asNoTracking: false)
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (record is null || record.UsedAt is not null || record.ExpiresAt <= now
            || !record.User.IsActive || record.User.ApprovedAt is null)
            throw new AppException(InvalidLinkMessage);

        if (BCrypt.Net.BCrypt.Verify(request.NewPassword, record.User.PasswordHash))
            throw new AppException("Nova lozinka mora se razlikovati od dosadasnje.");

        record.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        record.UsedAt = now;

        var sessions = await _uow.Repository<RefreshToken>()
            .Query(asNoTracking: false)
            .Where(x => x.UserId == record.UserId && x.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.RevokedAt = now;
            _uow.Repository<RefreshToken>().Update(session);
        }

        _uow.Repository<PasswordResetToken>().Update(record);
        await _uow.SaveChangesAsync(ct);

        var (html, text) = EmailTemplates.PasswordChanged(record.User.FirstName, now.ToLocalTime());

        try
        {
            await _email.SendAsync(
                new EmailMessage(record.User.Email, record.User.FirstName, "Lozinka je promenjena — Skladišnik", html, text), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Potvrda o promeni lozinke korisniku {UserId} nije poslata.", record.UserId);
        }
    }

    private static string Hash(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim().ToLowerInvariant())));

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
            return email;

        return $"{email[0]}{new string('•', Math.Min(at - 1, 6))}{email[at..]}";
    }
}
