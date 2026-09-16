using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Catalog;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Catalog;

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

        if (filter.CreatedFrom.HasValue)
            query = query.Where(x => x.CreatedAt >= filter.CreatedFrom);

        if (filter.CreatedTo.HasValue)
            query = query.Where(x => x.CreatedAt <= filter.CreatedTo);

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

        ValidateStockLimits(request.MinStock, request.MaxStock);

        if (await repo.ExistsAsync(x => x.Sku == request.Sku, ct))
            throw new AppException("Proizvod sa istim SKU kodom vec postoji.");

        if (!await _uow.Repository<Category>().ExistsAsync(x => x.Id == request.CategoryId, ct))
            throw new AppException("Kategorija ne postoji.");

        var product = new Product
        {
            Sku = request.Sku.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
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

        ValidateStockLimits(request.MinStock, request.MaxStock);

        if (await repo.ExistsAsync(x => x.Sku == request.Sku && x.Id != id, ct))
            throw new AppException("Proizvod sa istim SKU kodom vec postoji.");

        if (!await _uow.Repository<Category>().ExistsAsync(x => x.Id == request.CategoryId, ct))
            throw new AppException("Kategorija ne postoji.");

        product.Sku = request.Sku.Trim();
        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
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

    private static void ValidateStockLimits(decimal min, decimal max)
    {
        if (min < 0 || max < 0)
            throw new AppException("Minimalna i maksimalna zaliha ne mogu biti negativne.");

        if (max > 0 && min > max)
            throw new AppException("Minimalna zaliha ne moze biti veca od maksimalne.");
    }
}
