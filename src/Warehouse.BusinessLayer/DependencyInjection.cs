using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.BusinessLayer.Services.Auth;
using Warehouse.BusinessLayer.Services.Catalog;
using Warehouse.BusinessLayer.Services.Identity;
using Warehouse.BusinessLayer.Services.Inventory;
using Warehouse.BusinessLayer.Services.Orders;
using Warehouse.BusinessLayer.Services.Partners;
using Warehouse.BusinessLayer.Settings;

namespace Warehouse.BusinessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IStorageLocationService, StorageLocationService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
