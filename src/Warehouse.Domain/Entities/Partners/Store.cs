using Warehouse.Domain.Common;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Orders;

namespace Warehouse.Domain.Entities.Partners;

public class Store : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ManagerName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<StorageLocation> StorageLocations { get; set; } = new List<StorageLocation>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
