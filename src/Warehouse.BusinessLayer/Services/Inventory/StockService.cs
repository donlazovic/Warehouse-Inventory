using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.BusinessLayer.Realtime;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Inventory;

public class StockService : IStockService
{
    private readonly IUnitOfWork _uow;
    private readonly IEventCollector _events;

    public StockService(IUnitOfWork uow, IEventCollector events)
    {
        _uow = uow;
        _events = events;
    }

    public async Task<PagedResult<StockItemDto>> GetStockAsync(StockFilterRequest filter, CancellationToken ct = default)
    {
        var query = _uow.Repository<StockItem>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.Product.Name.Contains(term) || x.Product.Sku.Contains(term));
        }

        if (filter.ProductId.HasValue)
            query = query.Where(x => x.ProductId == filter.ProductId);

        if (filter.StorageLocationId.HasValue)
            query = query.Where(x => x.StorageLocationId == filter.StorageLocationId);

        if (filter.StoreId.HasValue)
            query = query.Where(x => x.StorageLocation.StoreId == filter.StoreId);

        if (filter.CategoryId.HasValue)
            query = query.Where(x => x.Product.CategoryId == filter.CategoryId);

        if (filter.LocationType.HasValue)
            query = query.Where(x => x.StorageLocation.LocationType == filter.LocationType);

        if (filter.OnlyBelowMinimum)
            query = query.Where(x => x.Product.IsActive && x.StorageLocation.IsActive
                                  && x.Quantity < (x.MinStockOverride ?? x.Product.MinStock));

        if (filter.OnlyInStock)
            query = query.Where(x => x.Quantity > 0);

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "quantity" => query.ApplySort(x => x.Quantity, filter.SortDesc),
            "location" => query.ApplySort(x => x.StorageLocation.Code, filter.SortDesc),
            "category" => query.ApplySort(x => x.Product.Category.Name, filter.SortDesc),
            "updatedat" => query.ApplySort(x => x.UpdatedAt, filter.SortDesc),
            _ => query.ApplySort(x => x.Product.Name, filter.SortDesc)
        };

        return await Project(query).ToPagedResultAsync(filter, ct);
    }

    public async Task<PagedResult<StockMovementDto>> GetMovementsAsync(StockMovementFilterRequest filter, CancellationToken ct = default)
    {
        var query = _uow.Repository<StockMovement>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.Product.Name.Contains(term)
                                  || x.Product.Sku.Contains(term)
                                  || (x.Order != null && x.Order.OrderNumber.Contains(term)));
        }

        if (filter.ProductId.HasValue)
            query = query.Where(x => x.ProductId == filter.ProductId);

        if (filter.LocationId.HasValue)
            query = query.Where(x => x.FromLocationId == filter.LocationId || x.ToLocationId == filter.LocationId);

        if (filter.MovementType.HasValue)
            query = query.Where(x => x.MovementType == filter.MovementType);

        if (filter.IssueReason.HasValue)
            query = query.Where(x => x.IssueReason == filter.IssueReason);

        if (filter.OrderId.HasValue)
            query = query.Where(x => x.OrderId == filter.OrderId);

        if (filter.UserId.HasValue)
            query = query.Where(x => x.UserId == filter.UserId);

        if (filter.DateFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.DateFrom);

        if (filter.DateTo.HasValue)
            query = query.Where(x => x.CreatedAt <= filter.DateTo);

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "product" => query.ApplySort(x => x.Product.Name, filter.SortDesc),
            "quantity" => query.ApplySort(x => x.Quantity, filter.SortDesc),
            "createdat" => query.ApplySort(x => x.CreatedAt, filter.SortDesc),
            _ => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
        };

        var projected = query.Select(x => new StockMovementDto(
            x.Id,
            x.MovementType,
            x.IssueReason,
            x.ProductId,
            x.Product.Sku,
            x.Product.Name,
            x.Product.UnitOfMeasure,
            x.Quantity,
            x.FromLocationId,
            x.FromLocation != null ? x.FromLocation.Name : null,
            x.ToLocationId,
            x.ToLocation != null ? x.ToLocation.Name : null,
            x.OrderId,
            x.Order != null ? x.Order.OrderNumber : null,
            x.User.FirstName + " " + x.User.LastName,
            x.Note,
            x.CreatedAt));

        return await projected.ToPagedResultAsync(filter, ct);
    }

    public async Task<StockItemDto> AdjustAsync(AdjustStockRequest request, int currentUserId, CancellationToken ct = default)
    {
        if (request.NewQuantity < 0)
            throw new AppException("Kolicina ne moze biti negativna.");

        if (string.IsNullOrWhiteSpace(request.Note))
            throw new AppException("Razlog korekcije je obavezan.");

        if (!await _uow.Repository<Product>().ExistsAsync(x => x.Id == request.ProductId, ct))
            throw new AppException("Proizvod nije pronadjen.", 404);

        if (!await _uow.Repository<StorageLocation>().ExistsAsync(x => x.Id == request.StorageLocationId && x.IsActive, ct))
            throw new AppException("Lokacija ne postoji ili je deaktivirana.", 404);

        await using var transaction = await _uow.BeginTransactionAsync(ct);

        var stockItem = await GetOrCreateStockItemAsync(request.ProductId, request.StorageLocationId, ct);
        var difference = request.NewQuantity - stockItem.Quantity;

        if (difference == 0)
            throw new AppException("Nova kolicina je ista kao trenutna.");

        var hasHistory = await _uow.Repository<StockMovement>().ExistsAsync(
            x => x.ProductId == request.ProductId
              && (x.FromLocationId == request.StorageLocationId || x.ToLocationId == request.StorageLocationId),
            ct);

        var movementType = !hasHistory && difference > 0 ? MovementType.InitialStock : MovementType.Adjustment;

        await ApplyMovementAsync(
            request.ProductId,
            Math.Abs(difference),
            movementType,
            fromLocationId: difference < 0 ? request.StorageLocationId : null,
            toLocationId: difference > 0 ? request.StorageLocationId : null,
            currentUserId,
            orderId: null,
            note: request.Note.Trim(),
            ct: ct);

        await _uow.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return await GetStockItemDtoAsync(stockItem.Id, ct);
    }

    public async Task<int> IssueAsync(IssueStockRequest request, int currentUserId, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(request.Reason))
            throw new AppException("Razlog izlaza nije ispravan.");

        if (request.Reason != IssueReason.Sale && string.IsNullOrWhiteSpace(request.Note))
            throw new AppException("Za otpis, lom i internu potrosnju napomena je obavezna.");

        if (request.Items is null || request.Items.Count == 0)
            throw new AppException("Izlaz mora imati bar jednu stavku.");

        if (request.Items.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
            throw new AppException("Isti proizvod se ne moze pojaviti u vise stavki.");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new AppException("Kolicina svake stavke mora biti veca od nule.");

        if (!await _uow.Repository<StorageLocation>().ExistsAsync(x => x.Id == request.StorageLocationId && x.IsActive, ct))
            throw new AppException("Lokacija ne postoji ili je deaktivirana.");

        await using var transaction = await _uow.BeginTransactionAsync(ct);

        foreach (var line in request.Items)
        {
            await ApplyMovementAsync(
                line.ProductId,
                line.Quantity,
                MovementType.Outbound,
                fromLocationId: request.StorageLocationId,
                toLocationId: null,
                currentUserId,
                orderId: null,
                note: request.Note?.Trim(),
                issueReason: request.Reason,
                ct: ct);
        }

        await _uow.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return request.Items.Count;
    }

    public async Task<StockItemDto> SetLimitsAsync(int stockItemId, SetStockLimitsRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<StockItem>();

        var stockItem = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == stockItemId, ct)
            ?? throw new AppException("Stavka zaliha nije pronadjena.", 404);

        if (request.MinStockOverride < 0 || request.MaxStockOverride < 0)
            throw new AppException("Granicne vrednosti ne mogu biti negativne.");

        if (request.MinStockOverride.HasValue && request.MaxStockOverride.HasValue
            && request.MaxStockOverride > 0 && request.MinStockOverride > request.MaxStockOverride)
            throw new AppException("Minimalna zaliha ne moze biti veca od maksimalne.");

        stockItem.MinStockOverride = request.MinStockOverride;
        stockItem.MaxStockOverride = request.MaxStockOverride;

        repo.Update(stockItem);
        await _uow.SaveChangesAsync(ct);

        return await GetStockItemDtoAsync(stockItemId, ct);
    }

    public async Task<StockReconciliationDto> GetReconciliationAsync(CancellationToken ct = default)
    {
        var movements = _uow.Repository<StockMovement>().Query();

        var incoming = await movements
            .Where(x => x.ToLocationId != null)
            .GroupBy(x => new { x.ProductId, LocationId = x.ToLocationId })
            .Select(g => new { g.Key.ProductId, g.Key.LocationId, Quantity = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var outgoing = await movements
            .Where(x => x.FromLocationId != null)
            .GroupBy(x => new { x.ProductId, LocationId = x.FromLocationId })
            .Select(g => new { g.Key.ProductId, g.Key.LocationId, Quantity = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var items = await _uow.Repository<StockItem>()
            .Query()
            .Select(x => new
            {
                x.ProductId,
                x.StorageLocationId,
                x.Quantity,
                ProductName = x.Product.Name,
                LocationName = x.StorageLocation.Name
            })
            .ToListAsync(ct);

        var incomingMap = incoming.ToDictionary(x => (x.ProductId, x.LocationId!.Value), x => x.Quantity);
        var outgoingMap = outgoing.ToDictionary(x => (x.ProductId, x.LocationId!.Value), x => x.Quantity);

        var mismatches = items
            .Select(item =>
            {
                var key = (item.ProductId, item.StorageLocationId);
                var expected = incomingMap.GetValueOrDefault(key) - outgoingMap.GetValueOrDefault(key);
                return new StockMismatchDto(
                    item.ProductId, item.ProductName, item.StorageLocationId, item.LocationName,
                    item.Quantity, expected);
            })
            .Where(x => x.RecordedQuantity != x.ExpectedQuantity)
            .ToList();

        return new StockReconciliationDto(DateTime.UtcNow, items.Count, mismatches.Count == 0, mismatches);
    }

    public async Task ApplyMovementAsync(
        int productId,
        decimal quantity,
        MovementType movementType,
        int? fromLocationId,
        int? toLocationId,
        int userId,
        int? orderId,
        string? note,
        IssueReason? issueReason = null,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw new AppException("Kolicina mora biti veca od nule.");

        if (fromLocationId is null && toLocationId is null)
            throw new AppException("Kretanje robe mora imati izvornu ili odredisnu lokaciju.");

        if (fromLocationId.HasValue)
        {
            var source = await GetOrCreateStockItemAsync(productId, fromLocationId.Value, ct);

            if (source.Quantity < quantity)
            {
                var name = await _uow.Repository<Product>()
                    .Query()
                    .Where(x => x.Id == productId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync(ct);

                throw new AppException(
                    $"Nedovoljna kolicina za proizvod \"{name}\". Dostupno: {source.Quantity:0.###}, potrebno: {quantity:0.###}.");
            }

            var before = source.Quantity;
            source.Quantity -= quantity;
            _uow.Repository<StockItem>().Update(source);
            _events.Add(new StockChangedEvent(productId, fromLocationId.Value, before, source.Quantity));
        }

        if (toLocationId.HasValue)
        {
            var destination = await GetOrCreateStockItemAsync(productId, toLocationId.Value, ct);
            var before = destination.Quantity;
            destination.Quantity += quantity;
            _uow.Repository<StockItem>().Update(destination);
            _events.Add(new StockChangedEvent(productId, toLocationId.Value, before, destination.Quantity));
        }

        await _uow.Repository<StockMovement>().AddAsync(new StockMovement
        {
            ProductId = productId,
            MovementType = movementType,
            IssueReason = issueReason,
            Quantity = quantity,
            FromLocationId = fromLocationId,
            ToLocationId = toLocationId,
            OrderId = orderId,
            UserId = userId,
            Note = note
        }, ct);
    }

    private async Task<StockItem> GetOrCreateStockItemAsync(int productId, int locationId, CancellationToken ct)
    {
        var repo = _uow.Repository<StockItem>();

        var existing = await repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.StorageLocationId == locationId, ct);

        if (existing is not null)
            return existing;

        var created = new StockItem
        {
            ProductId = productId,
            StorageLocationId = locationId,
            Quantity = 0
        };

        await repo.AddAsync(created, ct);
        await _uow.SaveChangesAsync(ct);

        return created;
    }

    private async Task<StockItemDto> GetStockItemDtoAsync(int id, CancellationToken ct)
        => await Project(_uow.Repository<StockItem>().Query().Where(x => x.Id == id)).FirstAsync(ct);

    private static IQueryable<StockItemDto> Project(IQueryable<StockItem> query)
        => query.Select(x => new StockItemDto(
            x.Id,
            x.ProductId,
            x.Product.Sku,
            x.Product.Name,
            x.Product.UnitOfMeasure,
            x.Product.Category.Name,
            x.StorageLocationId,
            x.StorageLocation.Code,
            x.StorageLocation.Name,
            x.StorageLocation.LocationType,
            x.StorageLocation.StoreId,
            x.StorageLocation.Store != null ? x.StorageLocation.Store.Name : null,
            x.Quantity,
            x.MinStockOverride ?? x.Product.MinStock,
            x.MaxStockOverride ?? x.Product.MaxStock,
            x.MinStockOverride,
            x.MaxStockOverride,
            x.Product.IsActive && x.StorageLocation.IsActive && x.Quantity < (x.MinStockOverride ?? x.Product.MinStock),
            x.UpdatedAt));
}
