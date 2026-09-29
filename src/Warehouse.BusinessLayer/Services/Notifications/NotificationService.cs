using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Notifications;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Notifications;

namespace Warehouse.BusinessLayer.Services.Notifications;

public class NotificationService : INotificationService
{
    private const int MaxTake = 50;

    private readonly IUnitOfWork _uow;

    public NotificationService(IUnitOfWork uow) => _uow = uow;

    public async Task<NotificationListDto> GetAsync(int userId, int take, CancellationToken ct = default)
    {
        var query = _uow.Repository<Notification>().Query().Where(x => x.UserId == userId);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(Math.Clamp(take, 1, MaxTake))
            .Select(x => new NotificationDto(x.Id, x.Type, x.Title, x.Message, x.Link, x.ReadAt != null, x.CreatedAt))
            .ToListAsync(ct);

        var unread = await query.CountAsync(x => x.ReadAt == null, ct);

        return new NotificationListDto(items, unread);
    }

    public async Task MarkReadAsync(int userId, int notificationId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Notification>();

        var notification = await repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, ct)
            ?? throw new AppException("Obavestenje nije pronadjeno.", 404);

        if (notification.ReadAt is not null)
            return;

        notification.ReadAt = DateTime.UtcNow;
        repo.Update(notification);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<int> MarkAllReadAsync(int userId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Notification>();

        var unread = await repo.Query(asNoTracking: false)
            .Where(x => x.UserId == userId && x.ReadAt == null)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.ReadAt = now;
            repo.Update(notification);
        }

        await _uow.SaveChangesAsync(ct);
        return unread.Count;
    }
}
