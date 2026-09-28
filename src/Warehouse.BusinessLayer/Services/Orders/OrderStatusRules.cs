using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Orders;

public static class OrderStatusRules
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Transitions = new()
    {
        [OrderStatus.Draft] = new[] { OrderStatus.PendingApproval, OrderStatus.Cancelled },
        [OrderStatus.PendingApproval] = new[] { OrderStatus.Approved, OrderStatus.Draft, OrderStatus.Cancelled },
        [OrderStatus.Approved] = new[] { OrderStatus.InProgress, OrderStatus.Cancelled },
        [OrderStatus.InProgress] = new[] { OrderStatus.Completed, OrderStatus.Cancelled },
        [OrderStatus.Completed] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
    };

    public static IReadOnlyList<OrderStatus> AllowedNext(OrderStatus current)
        => Transitions.TryGetValue(current, out var next) ? next : Array.Empty<OrderStatus>();

    public static bool CanTransition(OrderStatus from, OrderStatus to)
        => AllowedNext(from).Contains(to);

    public static string RequiredPermission(OrderStatus to) => to switch
    {
        OrderStatus.Approved => "orders.approve",
        OrderStatus.Completed => "orders.execute",
        _ => "orders.update"
    };

    public static string Title(OrderStatus status) => status switch
    {
        OrderStatus.Draft => "Nacrt",
        OrderStatus.PendingApproval => "Ceka odobrenje",
        OrderStatus.Approved => "Odobren",
        OrderStatus.InProgress => "U realizaciji",
        OrderStatus.Completed => "Realizovan",
        OrderStatus.Cancelled => "Otkazan",
        _ => status.ToString()
    };
}
