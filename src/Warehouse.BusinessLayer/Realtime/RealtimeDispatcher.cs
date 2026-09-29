using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.DTOs.Notifications;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Notifications;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Realtime;

public class RealtimeDispatcher : IRealtimeDispatcher
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("sr-Latn-RS");

    private readonly IEventCollector _collector;
    private readonly IRealtimePublisher _publisher;
    private readonly IUnitOfWork _uow;

    public RealtimeDispatcher(IEventCollector collector, IRealtimePublisher publisher, IUnitOfWork uow)
    {
        _collector = collector;
        _publisher = publisher;
        _uow = uow;
    }

    public async Task FlushAsync(CancellationToken ct = default)
    {
        var events = _collector.Drain();
        if (events.Count == 0)
            return;

        var notifications = new List<Notification>();

        await HandleOrdersAsync(events.OfType<OrderChangedEvent>().ToList(), notifications, ct);
        await HandleStockAsync(events.OfType<StockChangedEvent>().ToList(), notifications, ct);
        await HandleUsersAsync(events, notifications, ct);

        if (notifications.Count == 0)
            return;

        var repo = _uow.Repository<Notification>();
        foreach (var notification in notifications)
            await repo.AddAsync(notification, ct);

        await _uow.SaveChangesAsync(ct);

        foreach (var notification in notifications)
        {
            var dto = new NotificationDto(
                notification.Id, notification.Type, notification.Title, notification.Message,
                notification.Link, false, notification.CreatedAt);

            await _publisher.ToUserAsync(notification.UserId, RealtimeMethods.Notification, dto, ct);
        }
    }

    private async Task HandleOrdersAsync(List<OrderChangedEvent> events, List<Notification> notifications, CancellationToken ct)
    {
        if (events.Count == 0)
            return;

        await _publisher.ToPermissionAsync(
            "orders.view",
            RealtimeMethods.OrdersChanged,
            new { orderIds = events.Select(x => x.OrderId).Distinct().ToList() },
            ct);

        foreach (var change in events.Where(x => x.PreviousStatus.HasValue && x.PreviousStatus != x.CurrentStatus))
        {
            var order = await _uow.Repository<Order>()
                .Query()
                .Where(x => x.Id == change.OrderId)
                .Select(x => new
                {
                    x.Id,
                    x.OrderNumber,
                    x.CreatedByUserId,
                    x.TotalValue,
                    Counterparty = x.Supplier != null ? x.Supplier.Name : x.Store != null ? x.Store.Name : null
                })
                .FirstOrDefaultAsync(ct);

            if (order is null)
                continue;

            var summary = $"{order.OrderNumber} · {order.Counterparty ?? "—"} · {order.TotalValue.ToString("#,##0.00", Culture)} RSD";
            var link = $"/nalozi?nalog={order.Id}";

            var (type, title, recipients) = change.CurrentStatus switch
            {
                OrderStatus.PendingApproval => (
                    NotificationType.OrderAwaitingApproval,
                    "Nalog ceka odobrenje",
                    await UsersWithPermissionAsync("orders.approve", ct)),
                OrderStatus.Approved => (
                    NotificationType.OrderApproved,
                    "Nalog je odobren i spreman za realizaciju",
                    (await UsersWithPermissionAsync("orders.execute", ct)).Append(order.CreatedByUserId).ToList()),
                OrderStatus.Draft => (
                    NotificationType.OrderReturned,
                    "Nalog je vracen na doradu",
                    new List<int> { order.CreatedByUserId }),
                OrderStatus.Completed => (
                    NotificationType.OrderCompleted,
                    "Nalog je realizovan",
                    new List<int> { order.CreatedByUserId }),
                OrderStatus.Cancelled => (
                    NotificationType.OrderCancelled,
                    "Nalog je otkazan",
                    new List<int> { order.CreatedByUserId }),
                _ => (default(NotificationType), string.Empty, new List<int>())
            };

            foreach (var userId in recipients.Distinct().Where(id => id != change.ActorUserId))
                notifications.Add(Create(userId, type, title, summary, link));
        }
    }

    private async Task HandleStockAsync(List<StockChangedEvent> events, List<Notification> notifications, CancellationToken ct)
    {
        if (events.Count == 0)
            return;

        await _publisher.ToPermissionAsync(
            "stock.view",
            RealtimeMethods.StockChanged,
            new
            {
                productIds = events.Select(x => x.ProductId).Distinct().ToList(),
                locationIds = events.Select(x => x.LocationId).Distinct().ToList()
            },
            ct);

        var changes = events
            .GroupBy(x => (x.ProductId, x.LocationId))
            .Select(g => (g.Key.ProductId, g.Key.LocationId, Before: g.First().Before, After: g.Last().After))
            .Where(x => x.After < x.Before)
            .ToList();

        List<int>? recipients = null;

        foreach (var change in changes)
        {
            var info = await _uow.Repository<StockItem>()
                .Query()
                .Where(x => x.ProductId == change.ProductId && x.StorageLocationId == change.LocationId)
                .Select(x => new
                {
                    Min = x.MinStockOverride ?? x.Product.MinStock,
                    x.Product.Name,
                    x.Product.UnitOfMeasure,
                    x.StorageLocation.Code,
                    StoreName = x.StorageLocation.Store != null ? x.StorageLocation.Store.Name : null
                })
                .FirstOrDefaultAsync(ct);

            if (info is null || info.Min <= 0)
                continue;

            var crossedMinimum = change.Before >= info.Min && change.After < info.Min;
            if (!crossedMinimum)
                continue;

            recipients ??= await UsersWithPermissionAsync("orders.create", ct);

            var message =
                $"{info.StoreName ?? info.Code}: {change.After.ToString("#,##0.###", Culture)} " +
                $"(minimum {info.Min.ToString("#,##0.###", Culture)})";

            foreach (var userId in recipients)
                notifications.Add(Create(userId, NotificationType.LowStock, $"Niska zaliha: {info.Name}", message, "/zalihe"));
        }
    }

    private async Task HandleUsersAsync(IReadOnlyList<RealtimeEvent> events, List<Notification> notifications, CancellationToken ct)
    {
        var registered = events.OfType<UserRegisteredEvent>().ToList();
        var pendingChanged = registered.Count > 0 || events.OfType<PendingUsersChangedEvent>().Any();

        if (pendingChanged)
            await _publisher.ToPermissionAsync("users.view", RealtimeMethods.PendingUsersChanged, new { }, ct);

        if (registered.Count == 0)
            return;

        var approvers = await UsersWithPermissionAsync("users.update", ct);

        foreach (var registration in registered)
        {
            var user = await _uow.Repository<User>()
                .Query()
                .Where(x => x.Id == registration.UserId)
                .Select(x => new { x.FirstName, x.LastName, x.Email })
                .FirstOrDefaultAsync(ct);

            if (user is null)
                continue;

            foreach (var userId in approvers)
            {
                notifications.Add(Create(
                    userId,
                    NotificationType.UserRegistered,
                    "Novi zahtev za pristup",
                    $"{user.FirstName} {user.LastName} ({user.Email})",
                    "/korisnici"));
            }
        }
    }

    private Task<List<int>> UsersWithPermissionAsync(string permission, CancellationToken ct)
        => _uow.Repository<User>()
            .Query()
            .Where(x => x.IsActive
                     && x.ApprovedAt != null
                     && x.Role.RolePermissions.Any(rp => rp.Permission.Code == permission))
            .Select(x => x.Id)
            .ToListAsync(ct);

    private static Notification Create(int userId, NotificationType type, string title, string message, string link)
        => new()
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link
        };
}
