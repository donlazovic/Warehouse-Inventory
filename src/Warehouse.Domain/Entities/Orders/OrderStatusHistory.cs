using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Enums;

namespace Warehouse.Domain.Entities.Orders;

public class OrderStatusHistory : BaseEntity
{
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Note { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ChangedByUserId { get; set; }
    public User ChangedByUser { get; set; } = null!;
}
