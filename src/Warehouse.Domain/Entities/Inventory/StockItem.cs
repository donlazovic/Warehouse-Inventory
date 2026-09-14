using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Catalog;

namespace Warehouse.Domain.Entities.Inventory;

public class StockItem : BaseEntity
{
    public decimal Quantity { get; set; }

    public decimal? MinStockOverride { get; set; }
    public decimal? MaxStockOverride { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int StorageLocationId { get; set; }
    public StorageLocation StorageLocation { get; set; } = null!;

    public decimal EffectiveMinStock => MinStockOverride ?? Product.MinStock;
    public decimal EffectiveMaxStock => MaxStockOverride ?? Product.MaxStock;
    public bool IsBelowMinimum => Quantity < EffectiveMinStock;
}
