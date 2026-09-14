namespace Warehouse.Domain.Enums;

public enum OrderStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6
}
