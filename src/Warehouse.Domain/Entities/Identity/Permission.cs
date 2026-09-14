using Warehouse.Domain.Common;
using Warehouse.Domain.Enums;

namespace Warehouse.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public PermissionAction Action { get; set; }

    public int? ParentId { get; set; }
    public Permission? Parent { get; set; }
    public ICollection<Permission> Children { get; set; } = new List<Permission>();

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
