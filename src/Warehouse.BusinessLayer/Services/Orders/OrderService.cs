using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Orders;
using Warehouse.BusinessLayer.Services.Inventory;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Entities.Orders;
using Warehouse.Domain.Entities.Partners;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Orders;

public class OrderService : IOrderService
{
    private static readonly OrderStatus[] KanbanStatuses =
    {
        OrderStatus.Draft,
        OrderStatus.PendingApproval,
        OrderStatus.Approved,
        OrderStatus.InProgress,
        OrderStatus.Completed
    };

    private readonly IUnitOfWork _uow;
    private readonly IStockService _stockService;

    public OrderService(IUnitOfWork uow, IStockService stockService)
    {
        _uow = uow;
        _stockService = stockService;
    }

    public async Task<PagedResult<OrderDto>> GetPagedAsync(OrderFilterRequest filter, CancellationToken ct = default)
    {
        var query = ApplyFilter(_uow.Repository<Order>().Query(), filter);

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "ordernumber" => query.ApplySort(x => x.OrderNumber, filter.SortDesc),
            "totalvalue" => query.ApplySort(x => x.TotalValue, filter.SortDesc),
            "status" => query.ApplySort(x => x.Status, filter.SortDesc),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        return await Project(query).ToPagedResultAsync(filter, ct);
    }

    public async Task<List<KanbanColumnDto>> GetKanbanAsync(OrderFilterRequest filter, CancellationToken ct = default)
    {
        var baseQuery = ApplyFilter(_uow.Repository<Order>().Query(), filter);

        var totals = await baseQuery
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Value = g.Sum(x => x.TotalValue) })
            .ToListAsync(ct);

        var columns = new List<KanbanColumnDto>();

        foreach (var status in KanbanStatuses)
        {
            var orders = await Project(baseQuery.Where(x => x.Status == status).OrderByDescending(x => x.CreatedAt))
                .Take(50)
                .ToListAsync(ct);

            var total = totals.FirstOrDefault(x => x.Status == status);

            columns.Add(new KanbanColumnDto(
                status,
                OrderStatusRules.Title(status),
                total?.Count ?? 0,
                total?.Value ?? 0m,
                orders));
        }

        return columns;
    }

    public async Task<OrderDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var order = await Project(_uow.Repository<Order>().Query().Where(x => x.Id == id)).FirstOrDefaultAsync(ct)
            ?? throw new AppException("Nalog nije pronadjen.", 404);

        var items = await _uow.Repository<OrderItem>()
            .Query()
            .Where(x => x.OrderId == id)
            .OrderBy(x => x.Id)
            .Select(x => new OrderItemDto(
                x.Id, x.ProductId, x.Product.Sku, x.Product.Name,
                x.Product.UnitOfMeasure, x.Quantity, x.UnitPrice, x.LineTotal))
            .ToListAsync(ct);

        var history = await _uow.Repository<OrderStatusHistory>()
            .Query()
            .Where(x => x.OrderId == id)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new OrderStatusHistoryDto(
                x.FromStatus, x.ToStatus,
                x.ChangedByUser.FirstName + " " + x.ChangedByUser.LastName,
                x.Note, x.CreatedAt))
            .ToListAsync(ct);

        return new OrderDetailDto(order, items, history, OrderStatusRules.AllowedNext(order.Status));
    }

    public async Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, int currentUserId, CancellationToken ct = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new AppException("Nalog mora imati bar jednu stavku.");

        var sourceLocationId = Normalize(request.SourceLocationId);
        var destinationLocationId = Normalize(request.DestinationLocationId);
        var supplierId = Normalize(request.SupplierId);
        var storeId = Normalize(request.StoreId);

        await ValidatePartiesAsync(request.OrderType, supplierId, storeId, sourceLocationId, destinationLocationId, ct);

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumberAsync(request.OrderType, ct),
            OrderType = request.OrderType,
            Status = OrderStatus.Draft,
            SupplierId = supplierId,
            StoreId = storeId,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            Note = request.Note?.Trim(),
            CreatedByUserId = currentUserId
        };

        order.Items = await BuildItemsAsync(request.Items, ct);
        order.TotalValue = order.Items.Sum(x => x.LineTotal);

        order.StatusHistory.Add(new OrderStatusHistory
        {
            FromStatus = null,
            ToStatus = OrderStatus.Draft,
            ChangedByUserId = currentUserId,
            Note = "Nalog kreiran"
        });

        await _uow.Repository<Order>().AddAsync(order, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(order.Id, ct);
    }

    public async Task<OrderDetailDto> UpdateAsync(int id, UpdateOrderRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Order>();

        var order = await repo.Query(asNoTracking: false)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Nalog nije pronadjen.", 404);

        if (order.Status != OrderStatus.Draft)
            throw new AppException("Izmena je moguca samo dok je nalog u statusu nacrta.");

        if (request.Items is null || request.Items.Count == 0)
            throw new AppException("Nalog mora imati bar jednu stavku.");

        var sourceLocationId = Normalize(request.SourceLocationId);
        var destinationLocationId = Normalize(request.DestinationLocationId);
        var supplierId = Normalize(request.SupplierId);
        var storeId = Normalize(request.StoreId);

        await ValidatePartiesAsync(order.OrderType, supplierId, storeId, sourceLocationId, destinationLocationId, ct);

        foreach (var item in order.Items.ToList())
            _uow.Repository<OrderItem>().Remove(item);

        order.Items = await BuildItemsAsync(request.Items, ct);
        order.TotalValue = order.Items.Sum(x => x.LineTotal);
        order.SupplierId = supplierId;
        order.StoreId = storeId;
        order.SourceLocationId = sourceLocationId;
        order.DestinationLocationId = destinationLocationId;
        order.Note = request.Note?.Trim();

        repo.Update(order);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Order>();

        var order = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Nalog nije pronadjen.", 404);

        if (order.Status != OrderStatus.Draft)
            throw new AppException("Brisanje je moguce samo za naloge u statusu nacrta. Koristite storniranje.");

        repo.Remove(order);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<OrderDetailDto> ChangeStatusAsync(int id, ChangeOrderStatusRequest request, int currentUserId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Order>();

        var order = await repo.Query(asNoTracking: false)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Nalog nije pronadjen.", 404);

        if (!OrderStatusRules.CanTransition(order.Status, request.Status))
            throw new AppException(
                $"Prelazak iz statusa '{OrderStatusRules.Title(order.Status)}' u " +
                $"'{OrderStatusRules.Title(request.Status)}' nije dozvoljen.");

        await using var transaction = await _uow.BeginTransactionAsync(ct);

        var previousStatus = order.Status;

        if (request.Status == OrderStatus.Completed)
            await ExecuteStockMovementsAsync(order, currentUserId, ct);

        order.Status = request.Status;

        if (request.Status == OrderStatus.Approved)
        {
            order.ApprovedByUserId = currentUserId;
            order.ApprovedAt = DateTime.UtcNow;
        }

        if (request.Status == OrderStatus.Completed)
            order.CompletedAt = DateTime.UtcNow;

        order.StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = previousStatus,
            ToStatus = request.Status,
            ChangedByUserId = currentUserId,
            Note = request.Note?.Trim()
        });

        repo.Update(order);
        await _uow.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    private async Task ExecuteStockMovementsAsync(Order order, int currentUserId, CancellationToken ct)
    {
        if (order.Items.Count == 0)
            throw new AppException("Nalog bez stavki ne moze biti realizovan.");

        var movementType = order.OrderType == OrderType.Inbound
            ? MovementType.Inbound
            : order.SourceLocationId.HasValue && order.DestinationLocationId.HasValue
                ? MovementType.Transfer
                : MovementType.Outbound;

        foreach (var item in order.Items)
        {
            await _stockService.ApplyMovementAsync(
                item.ProductId,
                item.Quantity,
                movementType,
                order.SourceLocationId,
                order.DestinationLocationId,
                currentUserId,
                order.Id,
                $"Realizacija naloga {order.OrderNumber}",
                ct);
        }
    }

    private async Task<List<OrderItem>> BuildItemsAsync(List<SaveOrderItemRequest> requested, CancellationToken ct)
    {
        var productIds = requested.Select(x => x.ProductId).Distinct().ToList();

        var products = await _uow.Repository<Product>()
            .Query()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Price, x.IsActive, x.Name })
            .ToListAsync(ct);

        if (products.Count != productIds.Count)
            throw new AppException("Jedan ili vise proizvoda ne postoji.");

        var inactive = products.FirstOrDefault(x => !x.IsActive);
        if (inactive is not null)
            throw new AppException($"Proizvod '{inactive.Name}' je deaktiviran i ne moze biti u nalogu.");

        var items = new List<OrderItem>();

        foreach (var line in requested)
        {
            if (line.Quantity <= 0)
                throw new AppException("Kolicina stavke mora biti veca od nule.");

            var product = products.First(x => x.Id == line.ProductId);
            var unitPrice = line.UnitPrice ?? product.Price;

            if (unitPrice < 0)
                throw new AppException("Cena stavke ne moze biti negativna.");

            items.Add(new OrderItem
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                LineTotal = Math.Round(line.Quantity * unitPrice, 2)
            });
        }

        return items;
    }

    private async Task ValidatePartiesAsync(
        OrderType orderType, int? supplierId, int? storeId,
        int? sourceLocationId, int? destinationLocationId, CancellationToken ct)
    {
        if (orderType == OrderType.Inbound)
        {
            if (supplierId is null)
                throw new AppException("Ulazni nalog mora imati dobavljaca.");

            if (destinationLocationId is null)
                throw new AppException("Ulazni nalog mora imati odredisnu lokaciju.");

            if (!await _uow.Repository<Supplier>().ExistsAsync(x => x.Id == supplierId && x.IsActive, ct))
                throw new AppException("Dobavljac ne postoji ili je deaktiviran.");
        }
        else
        {
            if (sourceLocationId is null)
                throw new AppException("Izlazni nalog mora imati izvornu lokaciju.");

            if (storeId is null)
                throw new AppException("Izlazni nalog mora imati prodajni objekat.");

            if (!await _uow.Repository<Store>().ExistsAsync(x => x.Id == storeId && x.IsActive, ct))
                throw new AppException("Prodajni objekat ne postoji ili je deaktiviran.");

            if (destinationLocationId is null)
                throw new AppException("Izlazni nalog mora imati odredisnu lokaciju objekta.");
        }

        if (sourceLocationId.HasValue && sourceLocationId == destinationLocationId)
            throw new AppException("Izvorna i odredisna lokacija ne mogu biti iste.");

        var locationRepo = _uow.Repository<StorageLocation>();

        if (sourceLocationId.HasValue && !await locationRepo.ExistsAsync(x => x.Id == sourceLocationId && x.IsActive, ct))
            throw new AppException("Izvorna lokacija ne postoji ili je deaktivirana.");

        if (destinationLocationId.HasValue && !await locationRepo.ExistsAsync(x => x.Id == destinationLocationId && x.IsActive, ct))
            throw new AppException("Odredisna lokacija ne postoji ili je deaktivirana.");
    }

    private async Task<string> GenerateOrderNumberAsync(OrderType orderType, CancellationToken ct)
    {
        var prefix = orderType == OrderType.Inbound ? "ULZ" : "IZL";
        var year = DateTime.UtcNow.Year;
        var pattern = $"{prefix}-{year}-";

        var lastNumber = await _uow.Repository<Order>()
            .Query()
            .Where(x => x.OrderNumber.StartsWith(pattern))
            .OrderByDescending(x => x.OrderNumber)
            .Select(x => x.OrderNumber)
            .FirstOrDefaultAsync(ct);

        var next = 1;

        if (lastNumber is not null && int.TryParse(lastNumber[pattern.Length..], out var parsed))
            next = parsed + 1;

        return $"{pattern}{next:D5}";
    }

    private static int? Normalize(int? value) => value is > 0 ? value : null;

    private static IQueryable<Order> ApplyFilter(IQueryable<Order> query, OrderFilterRequest filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.OrderNumber.Contains(term)
                                  || (x.Note != null && x.Note.Contains(term)));
        }

        if (filter.OrderType.HasValue)
            query = query.Where(x => x.OrderType == filter.OrderType);

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status);

        if (filter.SupplierId.HasValue)
            query = query.Where(x => x.SupplierId == filter.SupplierId);

        if (filter.StoreId.HasValue)
            query = query.Where(x => x.StoreId == filter.StoreId);

        if (filter.CreatedFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.CreatedFrom);

        if (filter.CreatedTo.HasValue)
            query = query.Where(x => x.CreatedAt <= filter.CreatedTo);

        return query;
    }

    private static IQueryable<OrderDto> Project(IQueryable<Order> query)
        => query.Select(x => new OrderDto(
            x.Id,
            x.OrderNumber,
            x.OrderType,
            x.Status,
            x.TotalValue,
            x.Note,
            x.SupplierId,
            x.Supplier != null ? x.Supplier.Name : null,
            x.StoreId,
            x.Store != null ? x.Store.Name : null,
            x.SourceLocationId,
            x.SourceLocation != null ? x.SourceLocation.Name : null,
            x.DestinationLocationId,
            x.DestinationLocation != null ? x.DestinationLocation.Name : null,
            x.CreatedByUser.FirstName + " " + x.CreatedByUser.LastName,
            x.ApprovedByUser != null ? x.ApprovedByUser.FirstName + " " + x.ApprovedByUser.LastName : null,
            x.ApprovedAt,
            x.CompletedAt,
            x.Items.Count,
            x.CreatedAt));
}
