using Warehouse.BusinessLayer.DTOs.Identity;

namespace Warehouse.BusinessLayer.Services.Identity;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllAsync(CancellationToken ct = default);
    Task<RoleDetailDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<PermissionNodeDto>> GetPermissionTreeAsync(CancellationToken ct = default);
    Task<RoleDetailDto> UpdateAsync(int id, UpdateRoleRequest request, CancellationToken ct = default);
}
