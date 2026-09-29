namespace Warehouse.BusinessLayer.Realtime;

public interface IRealtimePublisher
{
    Task ToPermissionAsync(string permission, string method, object payload, CancellationToken ct = default);
    Task ToUserAsync(int userId, string method, object payload, CancellationToken ct = default);
}

public interface IRealtimeDispatcher
{
    Task FlushAsync(CancellationToken ct = default);
}
