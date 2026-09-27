using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Identity;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Identity;

public class RoleService : IRoleService
{
    private const string AdminRoleName = "Admin";

    private readonly IUnitOfWork _uow;

    public RoleService(IUnitOfWork uow) => _uow = uow;

    public Task<List<RoleDto>> GetAllAsync(CancellationToken ct = default)
        => _uow.Repository<Role>()
            .Query()
            .OrderBy(x => x.Id)
            .Select(x => new RoleDto(
                x.Id,
                x.Name,
                x.Description,
                x.IsSystemRole,
                x.Name == AdminRoleName,
                x.Users.Count,
                x.RolePermissions.Count))
            .ToListAsync(ct);

    public async Task<RoleDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var role = await _uow.Repository<Role>()
            .Query()
            .Where(x => x.Id == id)
            .Select(x => new RoleDto(
                x.Id, x.Name, x.Description, x.IsSystemRole, x.Name == AdminRoleName,
                x.Users.Count, x.RolePermissions.Count))
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException("Uloga nije pronadjena.", 404);

        var permissionIds = await _uow.Repository<Permission>()
            .Query()
            .Where(x => x.RolePermissions.Any(rp => rp.RoleId == id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        return new RoleDetailDto(role, permissionIds);
    }

    public async Task<List<PermissionNodeDto>> GetPermissionTreeAsync(CancellationToken ct = default)
    {
        var all = await _uow.Repository<Permission>()
            .Query()
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Code, x.Name, x.Module, x.Action, x.ParentId })
            .ToListAsync(ct);

        var lookup = all.ToLookup(x => x.ParentId);

        List<PermissionNodeDto> Build(int? parentId) =>
            lookup[parentId]
                .Select(x => new PermissionNodeDto(x.Id, x.Code, x.Name, x.Module, x.Action, Build(x.Id)))
                .ToList();

        return Build(null);
    }

    public async Task<RoleDetailDto> UpdateAsync(int id, UpdateRoleRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Role>();

        var role = await repo.Query(asNoTracking: false)
            .Include(x => x.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Uloga nije pronadjena.", 404);

        if (role.Name == AdminRoleName)
            throw new AppException("Dozvole administratorske uloge se ne mogu menjati.");

        var requested = (request.PermissionIds ?? new List<int>()).Distinct().ToList();

        var validPermissions = await _uow.Repository<Permission>()
            .Query()
            .Where(x => requested.Contains(x.Id) && x.Action != PermissionAction.Module)
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (validPermissions.Count != requested.Count)
            throw new AppException("Lista sadrzi dozvolu koja ne postoji ili je cvor modula.");

        var current = role.RolePermissions.ToList();

        foreach (var existing in current.Where(x => !validPermissions.Contains(x.PermissionId)))
            role.RolePermissions.Remove(existing);

        var currentIds = current.Select(x => x.PermissionId).ToHashSet();

        foreach (var permissionId in validPermissions.Where(x => !currentIds.Contains(x)))
        {
            role.RolePermissions.Add(new RolePermission
            {
                RoleId = id,
                PermissionId = permissionId,
                GrantedAt = DateTime.UtcNow
            });
        }

        role.Description = request.Description?.Trim();

        repo.Update(role);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }
}
