using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Reports;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Reports;

public class ReportService : IReportService
{
    private const int DashboardDays = 14;
    private const int TopIssuedDays = 30;

    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow) => _uow = uow;

    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var stock = _uow.Repository<StockItem>().Query();
        var movements = _uow.Repository<StockMovement>().Query();
        var orders = _uow.Repository<Order>().Query();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var previousMonthStart = monthStart.AddMonths(-1);

        var stockValue = await stock.SumAsync(x => (decimal?)(x.Quantity * x.Product.Price), ct) ?? 0m;

        var statusCounts = await orders
            .GroupBy(x => x.Status)
            .Select(g => new StatusCountDto(g.Key, g.Count()))
            .ToListAsync(ct);

        int CountOf(params OrderStatus[] statuses) =>
            statusCounts.Where(x => statuses.Contains(x.Status)).Sum(x => x.Count);

        var belowMinimum = await stock.CountAsync(x => x.Quantity < (x.MinStockOverride ?? x.Product.MinStock), ct);

        var completedThisMonth = await orders.CountAsync(x => x.CompletedAt >= monthStart, ct);
        var completedLastMonth = await orders.CountAsync(
            x => x.CompletedAt >= previousMonthStart && x.CompletedAt < monthStart, ct);

        var issuedThisMonth = await movements
            .Where(x => x.MovementType == MovementType.Outbound && x.CreatedAt >= monthStart)
            .SumAsync(x => (decimal?)(x.Quantity * x.Product.Price), ct) ?? 0m;

        var issuedLastMonth = await movements
            .Where(x => x.MovementType == MovementType.Outbound
                     && x.CreatedAt >= previousMonthStart && x.CreatedAt < monthStart)
            .SumAsync(x => (decimal?)(x.Quantity * x.Product.Price), ct) ?? 0m;

        var kpi = new KpiDto(
            stockValue,
            CountOf(OrderStatus.Draft, OrderStatus.PendingApproval, OrderStatus.Approved, OrderStatus.InProgress),
            CountOf(OrderStatus.PendingApproval),
            belowMinimum,
            completedThisMonth,
            completedLastMonth,
            issuedThisMonth,
            issuedLastMonth);

        var since = now.Date.AddDays(-(DashboardDays - 1));

        var flowRaw = await movements
            .Where(x => x.CreatedAt >= since)
            .Select(x => new { Day = x.CreatedAt.Date, x.MovementType, Value = x.Quantity * x.Product.Price })
            .GroupBy(x => new { x.Day, x.MovementType })
            .Select(g => new { g.Key.Day, g.Key.MovementType, Value = g.Sum(v => v.Value) })
            .ToListAsync(ct);

        var dailyFlow = Enumerable.Range(0, DashboardDays)
            .Select(offset => since.AddDays(offset))
            .Select(day => new DailyFlowDto(
                day,
                flowRaw.Where(x => x.Day == day && x.MovementType is MovementType.Inbound or MovementType.InitialStock).Sum(x => x.Value),
                flowRaw.Where(x => x.Day == day && x.MovementType == MovementType.Transfer).Sum(x => x.Value),
                flowRaw.Where(x => x.Day == day && x.MovementType == MovementType.Outbound).Sum(x => x.Value)))
            .ToList();

        var byCategory = await stock
            .Select(x => new { Category = x.Product.Category.Name, Value = x.Quantity * x.Product.Price })
            .GroupBy(x => x.Category)
            .Select(g => new CategoryValueDto(g.Key, g.Sum(v => v.Value)))
            .ToListAsync(ct);

        var topSince = now.AddDays(-TopIssuedDays);
        var topRaw = await movements
            .Where(x => x.MovementType == MovementType.Outbound && x.CreatedAt >= topSince)
            .Select(x => new { x.ProductId, x.Quantity, Value = x.Quantity * x.Product.Price })
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(v => v.Quantity), Value = g.Sum(v => v.Value) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync(ct);

        var topIssued = await ToTopProductsAsync(topRaw.Select(x => (x.ProductId, x.Quantity, x.Value)), ct);

        var lowStock = await stock
            .Where(x => x.Quantity < (x.MinStockOverride ?? x.Product.MinStock))
            .OrderBy(x => x.Quantity / (x.MinStockOverride ?? x.Product.MinStock))
            .Take(6)
            .Select(x => new LowStockDto(
                x.Id,
                x.Product.Name,
                x.StorageLocation.Code,
                x.StorageLocation.Store != null ? x.StorageLocation.Store.Name : null,
                x.Product.UnitOfMeasure,
                x.Quantity,
                x.MinStockOverride ?? x.Product.MinStock))
            .ToListAsync(ct);

        return new DashboardDto(
            kpi,
            dailyFlow,
            byCategory.OrderByDescending(x => x.Value).ToList(),
            topIssued,
            lowStock,
            statusCounts.OrderBy(x => x.Status).ToList());
    }

    public async Task<TurnoverReportDto> GetTurnoverAsync(TurnoverRequest request, CancellationToken ct = default)
    {
        var to = request.To ?? DateTime.UtcNow;
        var from = request.From ?? to.AddDays(-30);

        if (from > to)
            throw new AppException("Pocetni datum ne moze biti posle krajnjeg.");

        var location = request.LocationId;
        var scoped = Scope(_uow.Repository<StockMovement>().Query(), request.CategoryId, location);

        var opening = location.HasValue
            ? await scoped.Where(x => x.CreatedAt < from)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Net = g.Sum(x => x.ToLocationId == location ? x.Quantity : 0m)
                        - g.Sum(x => x.FromLocationId == location ? x.Quantity : 0m)
                })
                .ToListAsync(ct)
            : await scoped.Where(x => x.CreatedAt < from)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Net = g.Sum(x => x.ToLocationId != null ? x.Quantity : 0m)
                        - g.Sum(x => x.FromLocationId != null ? x.Quantity : 0m)
                })
                .ToListAsync(ct);

        var period = await scoped
            .Where(x => x.CreatedAt >= from && x.CreatedAt <= to)
            .Select(x => new { x.ProductId, x.MovementType, x.Quantity, x.FromLocationId, x.ToLocationId })
            .ToListAsync(ct);

        bool Enters(int? toLocation) => location.HasValue ? toLocation == location : toLocation.HasValue;
        bool Leaves(int? fromLocation) => location.HasValue ? fromLocation == location : fromLocation.HasValue;

        var productIds = opening.Select(x => x.ProductId).Concat(period.Select(x => x.ProductId)).Distinct().ToList();

        var products = await _uow.Repository<Product>()
            .Query()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Sku, x.Name, Category = x.Category.Name, x.UnitOfMeasure, x.Price })
            .ToListAsync(ct);

        var rows = products
            .Select(product =>
            {
                var lines = period.Where(x => x.ProductId == product.Id).ToList();
                var openingQty = opening.FirstOrDefault(x => x.ProductId == product.Id)?.Net ?? 0m;

                var received = lines
                    .Where(x => x.MovementType is MovementType.Inbound or MovementType.InitialStock && Enters(x.ToLocationId))
                    .Sum(x => x.Quantity);
                var transferredIn = lines
                    .Where(x => x.MovementType == MovementType.Transfer && Enters(x.ToLocationId))
                    .Sum(x => x.Quantity);
                var transferredOut = lines
                    .Where(x => x.MovementType == MovementType.Transfer && Leaves(x.FromLocationId))
                    .Sum(x => x.Quantity);
                var issued = lines
                    .Where(x => x.MovementType == MovementType.Outbound && Leaves(x.FromLocationId))
                    .Sum(x => x.Quantity);
                var adjustment = lines
                    .Where(x => x.MovementType == MovementType.Adjustment)
                    .Sum(x => (Enters(x.ToLocationId) ? x.Quantity : 0m) - (Leaves(x.FromLocationId) ? x.Quantity : 0m));

                var closing = openingQty + received + transferredIn - transferredOut - issued + adjustment;

                return new TurnoverRowDto(
                    product.Id, product.Sku, product.Name, product.Category, product.UnitOfMeasure,
                    openingQty, received, transferredIn, transferredOut, issued, adjustment, closing,
                    received * product.Price, issued * product.Price);
            })
            .Where(x => x.Opening != 0 || x.Received != 0 || x.TransferredIn != 0 || x.TransferredOut != 0
                     || x.Issued != 0 || x.AdjustmentNet != 0)
            .OrderBy(x => x.Name)
            .ToList();

        return new TurnoverReportDto(
            from, to, location,
            rows.Sum(x => x.ReceivedValue),
            rows.Sum(x => x.IssuedValue),
            rows);
    }

    public async Task<SnapshotReportDto> GetSnapshotAsync(SnapshotRequest request, CancellationToken ct = default)
    {
        var at = request.At ?? DateTime.UtcNow;
        var scoped = Scope(_uow.Repository<StockMovement>().Query(), request.CategoryId, request.LocationId)
            .Where(x => x.CreatedAt <= at);

        var incoming = await scoped
            .Where(x => x.ToLocationId != null)
            .GroupBy(x => new { x.ProductId, LocationId = x.ToLocationId!.Value })
            .Select(g => new { g.Key.ProductId, g.Key.LocationId, Quantity = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var outgoing = await scoped
            .Where(x => x.FromLocationId != null)
            .GroupBy(x => new { x.ProductId, LocationId = x.FromLocationId!.Value })
            .Select(g => new { g.Key.ProductId, g.Key.LocationId, Quantity = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var balances = incoming
            .Select(x => (x.ProductId, x.LocationId, x.Quantity))
            .Concat(outgoing.Select(x => (x.ProductId, x.LocationId, Quantity: -x.Quantity)))
            .Where(x => !request.LocationId.HasValue || x.LocationId == request.LocationId)
            .GroupBy(x => (x.ProductId, x.LocationId))
            .Select(g => (g.Key.ProductId, g.Key.LocationId, Quantity: g.Sum(v => v.Quantity)))
            .Where(x => x.Quantity != 0)
            .ToList();

        var productIds = balances.Select(x => x.ProductId).Distinct().ToList();
        var locationIds = balances.Select(x => x.LocationId).Distinct().ToList();

        var products = await _uow.Repository<Product>()
            .Query()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Sku, x.Name, Category = x.Category.Name, x.UnitOfMeasure, x.Price })
            .ToDictionaryAsync(x => x.Id, ct);

        var locations = await _uow.Repository<StorageLocation>()
            .Query()
            .Where(x => locationIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Code, x.Name })
            .ToDictionaryAsync(x => x.Id, ct);

        var rows = balances
            .Select(x =>
            {
                var product = products[x.ProductId];
                var location = locations[x.LocationId];
                return new SnapshotRowDto(
                    product.Id, product.Sku, product.Name, product.Category, product.UnitOfMeasure,
                    location.Id, location.Code, location.Name,
                    x.Quantity, product.Price, x.Quantity * product.Price);
            })
            .OrderBy(x => x.LocationCode)
            .ThenBy(x => x.Name)
            .ToList();

        return new SnapshotReportDto(at, rows.Sum(x => x.Value), rows);
    }

    public async Task<SupplierActivityDto> GetSupplierActivityAsync(
        int supplierId, SupplierActivityRequest request, CancellationToken ct = default)
    {
        var supplier = await _uow.Repository<Supplier>()
            .Query()
            .Where(x => x.Id == supplierId)
            .Select(x => new { x.Id, x.Name })
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException("Dobavljac nije pronadjen.", 404);

        var to = request.To ?? DateTime.UtcNow;
        var from = request.From ?? to.AddMonths(-6);

        var orders = await _uow.Repository<Order>()
            .Query()
            .Where(x => x.SupplierId == supplierId && x.CreatedAt >= from && x.CreatedAt <= to)
            .Select(x => new { x.Id, x.Status, x.TotalValue, x.CreatedAt, x.CompletedAt })
            .ToListAsync(ct);

        var completed = orders.Where(x => x.Status == OrderStatus.Completed && x.CompletedAt.HasValue).ToList();

        double? averageLeadDays = completed.Count == 0
            ? null
            : Math.Round(completed.Average(x => (x.CompletedAt!.Value - x.CreatedAt).TotalDays), 1);

        var monthly = completed
            .GroupBy(x => new { x.CompletedAt!.Value.Year, x.CompletedAt!.Value.Month })
            .Select(g => new MonthValueDto(g.Key.Year, g.Key.Month, g.Count(), g.Sum(x => x.TotalValue)))
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToList();

        var completedIds = completed.Select(x => x.Id).ToList();

        var topRaw = await _uow.Repository<OrderItem>()
            .Query()
            .Where(x => completedIds.Contains(x.OrderId))
            .GroupBy(x => x.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity), Value = g.Sum(x => x.LineTotal) })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync(ct);

        var topProducts = await ToTopProductsAsync(topRaw.Select(x => (x.ProductId, x.Quantity, x.Value)), ct);

        return new SupplierActivityDto(
            supplier.Id,
            supplier.Name,
            from,
            to,
            orders.Count,
            completed.Count,
            orders.Count(x => x.Status == OrderStatus.Cancelled),
            orders.Count(x => x.Status is not (OrderStatus.Completed or OrderStatus.Cancelled)),
            completed.Sum(x => x.TotalValue),
            averageLeadDays,
            monthly,
            topProducts);
    }

    private static IQueryable<StockMovement> Scope(IQueryable<StockMovement> query, int? categoryId, int? locationId)
    {
        if (categoryId.HasValue)
            query = query.Where(x => x.Product.CategoryId == categoryId);

        if (locationId.HasValue)
            query = query.Where(x => x.FromLocationId == locationId || x.ToLocationId == locationId);

        return query;
    }

    private async Task<List<TopProductDto>> ToTopProductsAsync(
        IEnumerable<(int ProductId, decimal Quantity, decimal Value)> source, CancellationToken ct)
    {
        var list = source.ToList();
        var ids = list.Select(x => x.ProductId).ToList();

        var products = await _uow.Repository<Product>()
            .Query()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.Sku, x.Name, x.UnitOfMeasure })
            .ToDictionaryAsync(x => x.Id, ct);

        return list
            .Where(x => products.ContainsKey(x.ProductId))
            .Select(x =>
            {
                var product = products[x.ProductId];
                return new TopProductDto(product.Id, product.Sku, product.Name, product.UnitOfMeasure, x.Quantity, x.Value);
            })
            .ToList();
    }
}
