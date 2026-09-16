using Microsoft.AspNetCore.Authorization;

namespace Warehouse.Api.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "PERMISSION_";

    public HasPermissionAttribute(string permission)
        => Policy = $"{PolicyPrefix}{permission}";
}
