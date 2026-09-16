using Warehouse.Domain.Entities.Identity;

namespace Warehouse.BusinessLayer.Services.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user, IReadOnlyList<string> permissions);
    string CreateRefreshToken();
}
