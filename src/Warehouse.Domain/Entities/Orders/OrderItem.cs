using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Catalog;

namespace Warehouse.Domain.Entities.Orders;

public class OrderItem : BaseEntity
{
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
