using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities.Identity;

namespace Warehouse.DataLayer.Seed;

public static class RoleSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin", Description = "Pun pristup sistemu", IsSystemRole = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "Menadzer", Description = "Odobravanje naloga i izvestaji", IsSystemRole = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 3, Name = "Magacioner", Description = "Realizacija naloga i korekcija zaliha", IsSystemRole = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 4, Name = "Viewer", Description = "Samo pregled", IsSystemRole = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { RoleId = 1, PermissionId = 2, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 3, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 4, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 5, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 7, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 8, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 9, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 10, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 12, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 13, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 14, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 16, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 17, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 18, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 19, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 20, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 21, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 22, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 24, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 25, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 26, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 27, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 29, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 30, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 31, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 32, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 34, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 35, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 37, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 38, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 39, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 1, PermissionId = 40, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 2, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 4, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 7, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 9, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 12, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 14, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 16, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 17, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 18, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 20, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 22, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 24, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 25, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 26, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 27, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 29, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 30, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 31, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 32, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 34, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 2, PermissionId = 35, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 2, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 7, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 12, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 13, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 16, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 17, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 18, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 21, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 24, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 29, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 3, PermissionId = 34, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 4, PermissionId = 2, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 4, PermissionId = 7, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 4, PermissionId = 12, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 4, PermissionId = 16, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 4, PermissionId = 24, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 4, PermissionId = 29, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new RolePermission { RoleId = 4, PermissionId = 34, GrantedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
