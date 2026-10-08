using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Catalog;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Catalog;
using Warehouse.Domain.Enums;

namespace Warehouse.BusinessLayer.Services.Catalog;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;

    public ProductService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductFilterRequest filter, int currentUserId, CancellationToken ct = default)
    {
        var query = _uow.Repository<Product>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Sku.Contains(term));
        }

        if (filter.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == filter.CategoryId);

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive);

        if (filter.DateFrom.HasValue && filter.DateTo.HasValue && filter.DateFrom > filter.DateTo)
            throw new AppException("Pocetni datum ne moze biti posle krajnjeg.");

        query = ApplyPeriod(query, filter);

        if (filter.OnlyFavorites)
            query = query.Where(x => x.FavoritedBy.Any(f => f.UserId == currentUserId));

        query = filter.StockStatus switch
        {
            StockStatusFilter.OutOfStock =>
                query.Where(x => x.StockItems.Sum(s => (decimal?)s.Quantity) == null
                              || x.StockItems.Sum(s => s.Quantity) <= 0),
            StockStatusFilter.BelowMinimum =>
                query.Where(x => x.StockItems.Sum(s => (decimal?)s.Quantity) < x.MinStock),
            StockStatusFilter.AboveMaximum =>
                query.Where(x => x.StockItems.Sum(s => (decimal?)s.Quantity) > x.MaxStock),
            StockStatusFilter.Normal =>
                query.Where(x => x.StockItems.Sum(s => (decimal?)s.Quantity) >= x.MinStock
                              && x.StockItems.Sum(s => (decimal?)s.Quantity) <= x.MaxStock),
            _ => query
        };

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "sku" => query.ApplySort(x => x.Sku, filter.SortDesc),
            "price" => query.ApplySort(x => x.Price, filter.SortDesc),
            "category" => query.ApplySort(x => x.Category.Name, filter.SortDesc),
            "createdat" => query.ApplySort(x => x.CreatedAt, filter.SortDesc),
            _ => query.ApplySort(x => x.Name, filter.SortDesc)
        };

        var projected = query.Select(x => new ProductDto(
            x.Id,
            x.Sku,
            x.Name,
            x.Description,
            x.UnitOfMeasure,
            x.Price,
            x.MinStock,
            x.MaxStock,
            x.IsActive,
            x.CategoryId,
            x.Category.Name,
            x.StockItems.Sum(s => (decimal?)s.Quantity) ?? 0m,
            x.FavoritedBy.Any(f => f.UserId == currentUserId),
            x.CreatedAt));

        return await projected.ToPagedResultAsync(filter, ct);
    }

    public async Task<ProductDetailDto> GetByIdAsync(int id, int currentUserId, CancellationToken ct = default)
    {
        var product = await _uow.Repository<Product>()
            .Query()
            .Where(x => x.Id == id)
            .Select(x => new ProductDto(
                x.Id, x.Sku, x.Name, x.Description, x.UnitOfMeasure, x.Price,
                x.MinStock, x.MaxStock, x.IsActive, x.CategoryId, x.Category.Name,
                x.StockItems.Sum(s => (decimal?)s.Quantity) ?? 0m,
                x.FavoritedBy.Any(f => f.UserId == currentUserId),
                x.CreatedAt))
            .FirstOrDefaultAsync(ct)
            ?? throw new AppException("Proizvod nije pronadjen.", 404);

        var rows = await _uow.Repository<Domain.Entities.Inventory.StockItem>()
            .Query()
            .Where(x => x.ProductId == id)
            .OrderBy(x => x.StorageLocation.Code)
            .Select(x => new
            {
                x.StorageLocationId,
                LocationCode = x.StorageLocation.Code,
                LocationName = x.StorageLocation.Name,
                x.StorageLocation.LocationType,
                StoreName = x.StorageLocation.Store != null ? x.StorageLocation.Store.Name : null,
                x.Quantity,
                x.MinStockOverride
            })
            .ToListAsync(ct);

        var stock = rows
            .Select(x =>
            {
                var min = x.MinStockOverride ?? product.MinStock;
                return new ProductStockByLocationDto(
                    x.StorageLocationId, x.LocationCode, x.LocationName,
                    x.LocationType, x.StoreName, x.Quantity, min, x.Quantity < min);
            })
            .ToList();

        return new ProductDetailDto(product, stock);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, int currentUserId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Product>();

        var sku = Guard.Required(request.Sku, "SKU", 50);
        var name = Guard.Required(request.Name, "Naziv", 200);
        var description = Guard.Optional(request.Description, "Opis", 1000);
        ValidateValues(request.UnitOfMeasure, request.Price, request.MinStock, request.MaxStock);

        if (await repo.ExistsAsync(x => x.Sku == sku, ct))
            throw new AppException("Proizvod sa istim SKU kodom vec postoji.");

        if (!await _uow.Repository<Category>().ExistsAsync(x => x.Id == request.CategoryId && x.IsActive, ct))
            throw new AppException("Kategorija ne postoji ili je deaktivirana.");

        var product = new Product
        {
            Sku = sku,
            Name = name,
            Description = description,
            UnitOfMeasure = request.UnitOfMeasure,
            Price = request.Price,
            MinStock = request.MinStock,
            MaxStock = request.MaxStock,
            CategoryId = request.CategoryId,
            IsActive = true
        };

        await repo.AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(product.Id, currentUserId, ct)).Product;
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request, int currentUserId, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Product>();

        var product = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Proizvod nije pronadjen.", 404);

        var sku = Guard.Required(request.Sku, "SKU", 50);
        var name = Guard.Required(request.Name, "Naziv", 200);
        var description = Guard.Optional(request.Description, "Opis", 1000);
        ValidateValues(request.UnitOfMeasure, request.Price, request.MinStock, request.MaxStock);

        if (await repo.ExistsAsync(x => x.Sku == sku && x.Id != id, ct))
            throw new AppException("Proizvod sa istim SKU kodom vec postoji.");

        var categoryOk = request.CategoryId == product.CategoryId
            ? await _uow.Repository<Category>().ExistsAsync(x => x.Id == request.CategoryId, ct)
            : await _uow.Repository<Category>().ExistsAsync(x => x.Id == request.CategoryId && x.IsActive, ct);

        if (!categoryOk)
            throw new AppException("Kategorija ne postoji ili je deaktivirana.");

        product.Sku = sku;
        product.Name = name;
        product.Description = description;
        product.UnitOfMeasure = request.UnitOfMeasure;
        product.Price = request.Price;
        product.MinStock = request.MinStock;
        product.MaxStock = request.MaxStock;
        product.CategoryId = request.CategoryId;
        product.IsActive = request.IsActive;

        repo.Update(product);
        await _uow.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, currentUserId, ct)).Product;
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Product>();

        var product = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Proizvod nije pronadjen.", 404);

        var hasStock = await _uow.Repository<Domain.Entities.Inventory.StockItem>()
            .ExistsAsync(x => x.ProductId == id && x.Quantity != 0, ct);

        if (hasStock)
            throw new AppException("Proizvod se ne moze obrisati jer postoji na zalihama.");

        var usedInOrders = await _uow.Repository<Domain.Entities.Orders.OrderItem>()
            .ExistsAsync(x => x.ProductId == id, ct);

        if (usedInOrders)
            throw new AppException("Proizvod se ne moze obrisati jer se koristi u nalozima. Deaktivirajte ga umesto toga.");

        var hasMovements = await _uow.Repository<Domain.Entities.Inventory.StockMovement>()
            .ExistsAsync(x => x.ProductId == id, ct);

        if (hasMovements)
            throw new AppException("Proizvod se ne moze obrisati jer postoji istorija kretanja robe. Deaktivirajte ga umesto toga.");

        var emptyStock = await _uow.Repository<Domain.Entities.Inventory.StockItem>()
            .Query(asNoTracking: false)
            .Where(x => x.ProductId == id)
            .ToListAsync(ct);

        foreach (var item in emptyStock)
            _uow.Repository<Domain.Entities.Inventory.StockItem>().Remove(item);

        repo.Remove(product);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> ToggleFavoriteAsync(int productId, int currentUserId, CancellationToken ct = default)
    {
        if (!await _uow.Repository<Product>().ExistsAsync(x => x.Id == productId, ct))
            throw new AppException("Proizvod nije pronadjen.", 404);

        var favorites = _uow.Repository<Product>();
        var context = favorites.Query(asNoTracking: false);

        var product = await context
            .Include(x => x.FavoritedBy)
            .FirstAsync(x => x.Id == productId, ct);

        var existing = product.FavoritedBy.FirstOrDefault(x => x.UserId == currentUserId);

        if (existing is not null)
        {
            product.FavoritedBy.Remove(existing);
            await _uow.SaveChangesAsync(ct);
            return false;
        }

        product.FavoritedBy.Add(new UserFavoriteProduct
        {
            ProductId = productId,
            UserId = currentUserId,
            CreatedAt = DateTime.UtcNow
        });

        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static IQueryable<Product> ApplyPeriod(IQueryable<Product> query, ProductFilterRequest filter)
    {
        var from = filter.DateFrom;
        var to = filter.DateTo;

        if (!from.HasValue && !to.HasValue)
            return query;

        if (filter.PeriodBasis == ProductPeriodBasis.Movement)
            return query.Where(x => x.StockMovements.Any(m =>
                (!from.HasValue || m.CreatedAt >= from) && (!to.HasValue || m.CreatedAt <= to)));

        if (from.HasValue)
            query = query.Where(x => x.CreatedAt >= from);

        if (to.HasValue)
            query = query.Where(x => x.CreatedAt <= to);

        return query;
    }

    private static void ValidateValues(UnitOfMeasure unit, decimal price, decimal min, decimal max)
    {
        Guard.Defined(unit, "Jedinica mere");
        Guard.Money(price, "Cena");
        ValidateStockLimits(min, max);
    }

    private static void ValidateStockLimits(decimal min, decimal max)
    {
        if (min < 0 || max < 0)
            throw new AppException("Minimalna i maksimalna zaliha ne mogu biti negativne.");

        if (max > 0 && min > max)
            throw new AppException("Minimalna zaliha ne moze biti veca od maksimalne.");
    }
}
