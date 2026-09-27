using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.DTOs.Identity;

public record PermissionNodeDto(
    int Id,
    string Code,
    string Name,
    string Module,
    PermissionAction Action,
    List<PermissionNodeDto> Children);

public record RoleDto(
    int Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    bool IsProtected,
    int UserCount,
    int PermissionCount);

public record RoleDetailDto(
    RoleDto Role,
    IReadOnlyList<int> PermissionIds);

public record UpdateRoleRequest(string? Description, List<int> PermissionIds);
