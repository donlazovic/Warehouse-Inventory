using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.DTOs.Notifications;

public record NotificationDto(
    int Id,
    NotificationType Type,
    string Title,
    string Message,
    string? Link,
    bool IsRead,
    DateTime CreatedAt);

public record NotificationListDto(IReadOnlyList<NotificationDto> Items, int UnreadCount);
