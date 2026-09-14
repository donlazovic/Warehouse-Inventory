using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Enums;

namespace Warehouse.Domain.Entities.Inventory;

public class StockMovement : BaseEntity
{
    public MovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public string? Note { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int? FromLocationId { get; set; }
    public StorageLocation? FromLocation { get; set; }

    public int? ToLocationId { get; set; }
    public StorageLocation? ToLocation { get; set; }

    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
