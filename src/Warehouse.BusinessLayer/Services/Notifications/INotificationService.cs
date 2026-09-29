using Warehouse.BusinessLayer.DTOs.Notifications;

namespace Warehouse.BusinessLayer.Services.Notifications;

public interface INotificationService
{
    Task<NotificationListDto> GetAsync(int userId, int take, CancellationToken ct = default);
    Task MarkReadAsync(int userId, int notificationId, CancellationToken ct = default);
    Task<int> MarkAllReadAsync(int userId, CancellationToken ct = default);
}
