using Microsoft.AspNetCore.SignalR;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.Realtime;

namespace Warehouse.Api.Realtime;

public class SignalRPublisher : IRealtimePublisher
{
    private readonly IHubContext<LiveHub> _hub;

    public SignalRPublisher(IHubContext<LiveHub> hub) => _hub = hub;

    public Task ToPermissionAsync(string permission, string method, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(LiveHub.PermissionGroup(permission)).SendAsync(method, payload, ct);

    public Task ToUserAsync(int userId, string method, object payload, CancellationToken ct = default)
        => _hub.Clients.User(userId.ToString()).SendAsync(method, payload, ct);
}

public class UidUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(AppClaimTypes.UserId)?.Value;
}
