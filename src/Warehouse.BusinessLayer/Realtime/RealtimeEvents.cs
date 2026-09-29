using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Realtime;

public abstract record RealtimeEvent;

public record OrderChangedEvent(
    int OrderId,
    OrderStatus? PreviousStatus,
    OrderStatus CurrentStatus,
    int? ActorUserId) : RealtimeEvent;

public record StockChangedEvent(int ProductId, int LocationId, decimal Before, decimal After) : RealtimeEvent;

public record UserRegisteredEvent(int UserId) : RealtimeEvent;

public record PendingUsersChangedEvent : RealtimeEvent;

public static class RealtimeMethods
{
    public const string OrdersChanged = "ordersChanged";
    public const string StockChanged = "stockChanged";
    public const string PendingUsersChanged = "pendingUsersChanged";
    public const string Notification = "notification";
}
