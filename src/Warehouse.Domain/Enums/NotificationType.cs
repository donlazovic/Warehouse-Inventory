namespace Warehouse.Domain.Enums;

public enum NotificationType
{
    OrderAwaitingApproval = 1,
    OrderApproved = 2,
    OrderReturned = 3,
    OrderCompleted = 4,
    OrderCancelled = 5,
    LowStock = 6,
    UserRegistered = 7
}
