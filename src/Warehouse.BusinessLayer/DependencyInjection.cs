using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using Warehouse.BusinessLayer.Realtime;
using Warehouse.BusinessLayer.Services.Auth;
using Warehouse.BusinessLayer.Services.Catalog;
using Warehouse.BusinessLayer.Services.Email;
using Warehouse.BusinessLayer.Services.Export;
using Warehouse.BusinessLayer.Services.Identity;
using Warehouse.BusinessLayer.Services.Inventory;
using Warehouse.BusinessLayer.Services.Notifications;
using Warehouse.BusinessLayer.Services.Orders;
using Warehouse.BusinessLayer.Services.Partners;
using Warehouse.BusinessLayer.Services.Reports;
using Warehouse.BusinessLayer.Settings;

namespace Warehouse.BusinessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<CompanySettings>(configuration.GetSection(CompanySettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<ClientSettings>(configuration.GetSection(ClientSettings.SectionName));

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordResetService, PasswordResetService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<ISupplierService, SupplierService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IStorageLocationService, StorageLocationService>();

        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IExportService, ExportService>();

        services.AddScoped<IEventCollector, EventCollector>();
        services.AddScoped<IRealtimeDispatcher, RealtimeDispatcher>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
