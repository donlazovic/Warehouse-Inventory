using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Enums;

namespace Warehouse.Domain.Entities.Notifications;

public class Notification : BaseEntity
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Link { get; set; }
    public DateTime? ReadAt { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
