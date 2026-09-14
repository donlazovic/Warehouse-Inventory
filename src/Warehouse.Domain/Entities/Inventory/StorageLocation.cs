using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.Domain.Entities.Inventory;

public class StorageLocation : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Zone { get; set; }
    public LocationType LocationType { get; set; }
    public bool IsActive { get; set; } = true;

    public int? StoreId { get; set; }
    public Store? Store { get; set; }

    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}
