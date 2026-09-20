using System.Security.Claims;
using Warehouse.BusinessLayer.Common;

namespace Warehouse.Api.Extensions;

public static class PermissionClaimExtensions
{
    public static bool HasPermission(this ClaimsPrincipal user, string permission)
        => user.Claims.Any(c => c.Type == AppClaimTypes.Permission && c.Value == permission);
}
