using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.Domain.Entities.Orders;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal TotalValue { get; set; }
    public string? Note { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? StoreId { get; set; }
    public Store? Store { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public int? ApprovedByUserId { get; set; }
    public User? ApprovedByUser { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
