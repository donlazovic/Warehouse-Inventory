using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Inventory;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Entities.Inventory;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Inventory;

public class StockService : IStockService
{
    private readonly IUnitOfWork _uow;

    public StockService(IUnitOfWork uow) => _uow = uow;

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

        if (filter.CategoryId.HasValue)
            query = query.Where(x => x.Product.CategoryId == filter.CategoryId);

        if (filter.LocationType.HasValue)
            query = query.Where(x => x.StorageLocation.LocationType == filter.LocationType);

        if (filter.OnlyBelowMinimum)
            query = query.Where(x => x.Quantity < (x.MinStockOverride ?? x.Product.MinStock));

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "quantity" => query.ApplySort(x => x.Quantity, filter.SortDesc),
            "location" => query.ApplySort(x => x.StorageLocation.Code, filter.SortDesc),
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
            query = query.Where(x => x.Product.Name.Contains(term) || x.Product.Sku.Contains(term));
        }

        if (filter.ProductId.HasValue)
            query = query.Where(x => x.ProductId == filter.ProductId);

        if (filter.LocationId.HasValue)
            query = query.Where(x => x.FromLocationId == filter.LocationId || x.ToLocationId == filter.LocationId);

        if (filter.MovementType.HasValue)
            query = query.Where(x => x.MovementType == filter.MovementType);

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
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var projected = query.Select(x => new StockMovementDto(
            x.Id,
            x.MovementType,
            x.ProductId,
            x.Product.Sku,
            x.Product.Name,
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

        if (!await _uow.Repository<Product>().ExistsAsync(x => x.Id == request.ProductId, ct))
            throw new AppException("Proizvod nije pronadjen.", 404);

        if (!await _uow.Repository<StorageLocation>().ExistsAsync(x => x.Id == request.StorageLocationId, ct))
            throw new AppException("Lokacija nije pronadjena.", 404);

        await using var transaction = await _uow.BeginTransactionAsync(ct);

        var stockItem = await GetOrCreateStockItemAsync(request.ProductId, request.StorageLocationId, ct);
        var difference = request.NewQuantity - stockItem.Quantity;

        if (difference == 0)
            throw new AppException("Nova kolicina je ista kao trenutna.");

        stockItem.Quantity = request.NewQuantity;
        _uow.Repository<StockItem>().Update(stockItem);

        await _uow.Repository<StockMovement>().AddAsync(new StockMovement
        {
            ProductId = request.ProductId,
            MovementType = MovementType.Adjustment,
            Quantity = Math.Abs(difference),
            FromLocationId = difference < 0 ? request.StorageLocationId : null,
            ToLocationId = difference > 0 ? request.StorageLocationId : null,
            UserId = currentUserId,
            Note = request.Note ?? "Rucna korekcija zaliha"
        }, ct);

        await _uow.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return await GetStockItemDtoAsync(stockItem.Id, ct);
    }

    public async Task<StockItemDto> SetLimitsAsync(int stockItemId, SetStockLimitsRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<StockItem>();

        var stockItem = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == stockItemId, ct)
            ?? throw new AppException("Stavka zaliha nije pronadjena.", 404);

        if (request.MinStockOverride < 0 || request.MaxStockOverride < 0)
            throw new AppException("Granicne vrednosti ne mogu biti negativne.");

        if (request.MinStockOverride.HasValue && request.MaxStockOverride.HasValue
            && request.MinStockOverride > request.MaxStockOverride)
            throw new AppException("Minimalna zaliha ne moze biti veca od maksimalne.");

        stockItem.MinStockOverride = request.MinStockOverride;
        stockItem.MaxStockOverride = request.MaxStockOverride;

        repo.Update(stockItem);
        await _uow.SaveChangesAsync(ct);

        return await GetStockItemDtoAsync(stockItemId, ct);
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
                var product = await _uow.Repository<Product>().GetByIdAsync(productId, ct);
                throw new AppException(
                    $"Nedovoljna kolicina na izvornoj lokaciji za proizvod {product?.Name}. " +
                    $"Dostupno: {source.Quantity}, potrebno: {quantity}.");
            }

            source.Quantity -= quantity;
            _uow.Repository<StockItem>().Update(source);
        }

        if (toLocationId.HasValue)
        {
            var destination = await GetOrCreateStockItemAsync(productId, toLocationId.Value, ct);
            destination.Quantity += quantity;
            _uow.Repository<StockItem>().Update(destination);
        }

        await _uow.Repository<StockMovement>().AddAsync(new StockMovement
        {
            ProductId = productId,
            MovementType = movementType,
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
            x.StorageLocation.Store != null ? x.StorageLocation.Store.Name : null,
            x.Quantity,
            x.MinStockOverride ?? x.Product.MinStock,
            x.MaxStockOverride ?? x.Product.MaxStock,
            x.Quantity < (x.MinStockOverride ?? x.Product.MinStock),
            x.UpdatedAt));
}
