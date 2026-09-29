using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Warehouse.BusinessLayer.Common;

namespace Warehouse.Api.Realtime;

[Authorize]
public class LiveHub : Hub
{
    public static string PermissionGroup(string permission) => $"perm:{permission}";

    public override async Task OnConnectedAsync()
    {
        var permissions = Context.User?.FindAll(AppClaimTypes.Permission).Select(c => c.Value) ?? [];

        foreach (var permission in permissions)
            await Groups.AddToGroupAsync(Context.ConnectionId, PermissionGroup(permission));

        await base.OnConnectedAsync();
    }
}
