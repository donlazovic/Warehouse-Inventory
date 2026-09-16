using System.Security.Claims;
using Warehouse.BusinessLayer.Common;

namespace Warehouse.Api.Extensions;

public static class CurrentUserExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(AppClaimTypes.UserId)?.Value;
        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Korisnik nije autentifikovan.");
    }
}
