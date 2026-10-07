using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Orders;
using Warehouse.BusinessLayer.Realtime;
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
        OrderStatus.Completed,
        OrderStatus.Cancelled
    };

    private readonly IUnitOfWork _uow;
    private readonly IStockService _stockService;
    private readonly IEventCollector _events;

    public OrderService(IUnitOfWork uow, IStockService stockService, IEventCollector events)
    {
        _uow = uow;
        _stockService = stockService;
        _events = events;
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

        _events.Add(new OrderChangedEvent(order.Id, null, OrderStatus.Draft, currentUserId));

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

        _events.Add(new OrderChangedEvent(id, order.Status, order.Status, null));

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

        _events.Add(new OrderChangedEvent(id, order.Status, order.Status, null));
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

        if (request.Status == OrderStatus.InProgress)
        {
            await ValidatePartiesAsync(
                order.OrderType, order.SupplierId, order.StoreId,
                order.SourceLocationId, order.DestinationLocationId, ct);

            if (order.OrderType == OrderType.Outbound)
                await EnsureStockAvailableAsync(order, ct);
        }

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

        _events.Add(new OrderChangedEvent(id, previousStatus, request.Status, currentUserId));

        return await GetByIdAsync(id, ct);
    }

    private async Task ExecuteStockMovementsAsync(Order order, int currentUserId, CancellationToken ct)
    {
        if (order.Items.Count == 0)
            throw new AppException("Nalog bez stavki ne moze biti realizovan.");

        var movementType = order.OrderType == OrderType.Inbound
            ? MovementType.Inbound
            : MovementType.Transfer;

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
                ct: ct);
        }
    }

    private async Task EnsureStockAvailableAsync(Order order, CancellationToken ct)
    {
        var productIds = order.Items.Select(x => x.ProductId).ToList();

        var available = await _uow.Repository<StockItem>()
            .Query()
            .Where(x => x.StorageLocationId == order.SourceLocationId && productIds.Contains(x.ProductId))
            .Select(x => new { x.ProductId, x.Quantity })
            .ToListAsync(ct);

        var shortIds = order.Items
            .Where(item => (available.FirstOrDefault(a => a.ProductId == item.ProductId)?.Quantity ?? 0m) < item.Quantity)
            .Select(item => item.ProductId)
            .ToList();

        if (shortIds.Count == 0)
            return;

        var names = await _uow.Repository<Product>()
            .Query()
            .Where(x => shortIds.Contains(x.Id))
            .Select(x => x.Name)
            .ToListAsync(ct);

        throw new AppException(
            $"Na izvornoj lokaciji nema dovoljno robe za: {string.Join(", ", names)}. " +
            "Realizacija ne moze da pocne dok se zalihe ne dopune.");
    }

    private async Task<List<OrderItem>> BuildItemsAsync(List<SaveOrderItemRequest> requested, CancellationToken ct)
    {
        if (requested.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
            throw new AppException("Isti proizvod se ne moze pojaviti u vise stavki. Saberite kolicine u jednoj stavci.");

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
        var locations = _uow.Repository<StorageLocation>().Query();

        if (orderType == OrderType.Inbound)
        {
            if (supplierId is null)
                throw new AppException("Ulazni nalog mora imati dobavljaca.");

            if (destinationLocationId is null)
                throw new AppException("Ulazni nalog mora imati odredisnu lokaciju.");

            if (sourceLocationId is not null)
                throw new AppException("Ulazni nalog nema izvornu lokaciju jer roba dolazi od dobavljaca.");

            if (storeId is not null)
                throw new AppException("Ulazni nalog se ne vezuje za prodajni objekat.");

            if (!await _uow.Repository<Supplier>().ExistsAsync(x => x.Id == supplierId && x.IsActive, ct))
                throw new AppException("Dobavljac ne postoji ili je deaktiviran.");

            var destination = await locations
                .Where(x => x.Id == destinationLocationId)
                .Select(x => new { x.LocationType, x.IsActive })
                .FirstOrDefaultAsync(ct);

            if (destination is null || !destination.IsActive)
                throw new AppException("Odredisna lokacija ne postoji ili je deaktivirana.");

            if (destination.LocationType != LocationType.CentralWarehouse)
                throw new AppException("Ulazni nalog prima robu samo u centralni magacin.");

            return;
        }

        if (storeId is null)
            throw new AppException("Izlazni nalog mora imati prodajni objekat.");

        if (sourceLocationId is null)
            throw new AppException("Izlazni nalog mora imati izvornu lokaciju.");

        if (destinationLocationId is null)
            throw new AppException("Izlazni nalog mora imati odredisnu lokaciju objekta.");

        if (supplierId is not null)
            throw new AppException("Izlazni nalog se ne vezuje za dobavljaca.");

        if (!await _uow.Repository<Store>().ExistsAsync(x => x.Id == storeId && x.IsActive, ct))
            throw new AppException("Prodajni objekat ne postoji ili je deaktiviran.");

        var source = await locations
            .Where(x => x.Id == sourceLocationId)
            .Select(x => new { x.LocationType, x.IsActive })
            .FirstOrDefaultAsync(ct);

        if (source is null || !source.IsActive)
            throw new AppException("Izvorna lokacija ne postoji ili je deaktivirana.");

        if (source.LocationType != LocationType.CentralWarehouse)
            throw new AppException("Izlazni nalog salje robu samo iz centralnog magacina.");

        var target = await locations
            .Where(x => x.Id == destinationLocationId)
            .Select(x => new { x.LocationType, x.StoreId, x.IsActive })
            .FirstOrDefaultAsync(ct);

        if (target is null || !target.IsActive)
            throw new AppException("Odredisna lokacija ne postoji ili je deaktivirana.");

        if (target.LocationType != LocationType.Store || target.StoreId != storeId)
            throw new AppException("Odredisna lokacija mora pripadati izabranom prodajnom objektu.");
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

        if (filter.CreatedFrom.HasValue && filter.CreatedTo.HasValue && filter.CreatedFrom > filter.CreatedTo)
            throw new AppException("Pocetni datum ne moze biti posle krajnjeg.");

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
            x.CreatedAt,
            x.SourceLocation != null ? x.SourceLocation.Code : null,
            x.DestinationLocation != null ? x.DestinationLocation.Code : null));
}
