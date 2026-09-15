using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities.Identity;
using Warehouse.Domain.Enums;

namespace Warehouse.DataLayer.Seed;

public static class PermissionSeed
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Code = "products", Name = "Proizvodi", Module = "Products", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 2, Code = "products.view", Name = "Pregled", Module = "Products", Action = PermissionAction.View, ParentId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 3, Code = "products.create", Name = "Kreiranje", Module = "Products", Action = PermissionAction.Create, ParentId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 4, Code = "products.update", Name = "Izmena", Module = "Products", Action = PermissionAction.Update, ParentId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 5, Code = "products.delete", Name = "Brisanje", Module = "Products", Action = PermissionAction.Delete, ParentId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 6, Code = "categories", Name = "Kategorije", Module = "Categories", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 7, Code = "categories.view", Name = "Pregled", Module = "Categories", Action = PermissionAction.View, ParentId = 6, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 8, Code = "categories.create", Name = "Kreiranje", Module = "Categories", Action = PermissionAction.Create, ParentId = 6, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 9, Code = "categories.update", Name = "Izmena", Module = "Categories", Action = PermissionAction.Update, ParentId = 6, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 10, Code = "categories.delete", Name = "Brisanje", Module = "Categories", Action = PermissionAction.Delete, ParentId = 6, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 11, Code = "stock", Name = "Zalihe", Module = "Stock", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 12, Code = "stock.view", Name = "Pregled", Module = "Stock", Action = PermissionAction.View, ParentId = 11, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 13, Code = "stock.update", Name = "Korekcija", Module = "Stock", Action = PermissionAction.Update, ParentId = 11, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 14, Code = "stock.export", Name = "Izvoz", Module = "Stock", Action = PermissionAction.Export, ParentId = 11, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 15, Code = "orders", Name = "Nalozi", Module = "Orders", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 16, Code = "orders.view", Name = "Pregled", Module = "Orders", Action = PermissionAction.View, ParentId = 15, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 17, Code = "orders.create", Name = "Kreiranje", Module = "Orders", Action = PermissionAction.Create, ParentId = 15, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 18, Code = "orders.update", Name = "Izmena", Module = "Orders", Action = PermissionAction.Update, ParentId = 15, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 19, Code = "orders.delete", Name = "Brisanje", Module = "Orders", Action = PermissionAction.Delete, ParentId = 15, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 20, Code = "orders.approve", Name = "Odobravanje", Module = "Orders", Action = PermissionAction.Approve, ParentId = 15, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 21, Code = "orders.execute", Name = "Realizacija", Module = "Orders", Action = PermissionAction.Execute, ParentId = 15, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 22, Code = "orders.export", Name = "Izvoz", Module = "Orders", Action = PermissionAction.Export, ParentId = 15, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 23, Code = "suppliers", Name = "Dobavljaci", Module = "Suppliers", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 24, Code = "suppliers.view", Name = "Pregled", Module = "Suppliers", Action = PermissionAction.View, ParentId = 23, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 25, Code = "suppliers.create", Name = "Kreiranje", Module = "Suppliers", Action = PermissionAction.Create, ParentId = 23, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 26, Code = "suppliers.update", Name = "Izmena", Module = "Suppliers", Action = PermissionAction.Update, ParentId = 23, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 27, Code = "suppliers.delete", Name = "Brisanje", Module = "Suppliers", Action = PermissionAction.Delete, ParentId = 23, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 28, Code = "stores", Name = "Prodajni objekti", Module = "Stores", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 29, Code = "stores.view", Name = "Pregled", Module = "Stores", Action = PermissionAction.View, ParentId = 28, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 30, Code = "stores.create", Name = "Kreiranje", Module = "Stores", Action = PermissionAction.Create, ParentId = 28, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 31, Code = "stores.update", Name = "Izmena", Module = "Stores", Action = PermissionAction.Update, ParentId = 28, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 32, Code = "stores.delete", Name = "Brisanje", Module = "Stores", Action = PermissionAction.Delete, ParentId = 28, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 33, Code = "reports", Name = "Izvestaji", Module = "Reports", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 34, Code = "reports.view", Name = "Pregled", Module = "Reports", Action = PermissionAction.View, ParentId = 33, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 35, Code = "reports.export", Name = "Izvoz", Module = "Reports", Action = PermissionAction.Export, ParentId = 33, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 36, Code = "users", Name = "Korisnici", Module = "Users", Action = PermissionAction.Module, ParentId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 37, Code = "users.view", Name = "Pregled", Module = "Users", Action = PermissionAction.View, ParentId = 36, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 38, Code = "users.create", Name = "Kreiranje", Module = "Users", Action = PermissionAction.Create, ParentId = 36, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 39, Code = "users.update", Name = "Izmena", Module = "Users", Action = PermissionAction.Update, ParentId = 36, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Permission { Id = 40, Code = "users.delete", Name = "Brisanje", Module = "Users", Action = PermissionAction.Delete, ParentId = 36, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
