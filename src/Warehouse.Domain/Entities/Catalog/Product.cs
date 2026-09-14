using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Enums;

namespace Warehouse.Domain.Entities.Catalog;

public class Product : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public decimal Price { get; set; }
    public decimal MinStock { get; set; }
    public decimal MaxStock { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<UserFavoriteProduct> FavoritedBy { get; set; } = new List<UserFavoriteProduct>();
}
