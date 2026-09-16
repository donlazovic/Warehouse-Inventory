using Microsoft.EntityFrameworkCore;
using Warehouse.BusinessLayer.Common;
using Warehouse.BusinessLayer.DTOs.Catalog;
using Warehouse.DataLayer.Repositories;
using Warehouse.Domain.Entities.Catalog;

namespace Warehouse.BusinessLayer.Services.Catalog;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _uow;

    public CategoryService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<CategoryDto>> GetPagedAsync(CategoryFilterRequest filter, CancellationToken ct = default)
    {
        var query = _uow.Repository<Category>().Query();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(x => x.Name.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }

        if (filter.ParentCategoryId.HasValue)
            query = query.Where(x => x.ParentCategoryId == filter.ParentCategoryId);

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive);

        query = filter.SortBy?.ToLowerInvariant() switch
        {
            "createdat" => query.ApplySort(x => x.CreatedAt, filter.SortDesc),
            _ => query.ApplySort(x => x.Name, filter.SortDesc)
        };

        var projected = query.Select(x => new CategoryDto(
            x.Id,
            x.Name,
            x.Description,
            x.IsActive,
            x.ParentCategoryId,
            x.ParentCategory != null ? x.ParentCategory.Name : null,
            x.Products.Count,
            x.CreatedAt));

        return await projected.ToPagedResultAsync(filter, ct);
    }

    public async Task<List<CategoryTreeDto>> GetTreeAsync(CancellationToken ct = default)
    {
        var all = await _uow.Repository<Category>()
            .Query()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name, x.IsActive, x.ParentCategoryId })
            .ToListAsync(ct);

        var lookup = all.ToLookup(x => x.ParentCategoryId);

        List<CategoryTreeDto> Build(int? parentId) =>
            lookup[parentId]
                .Select(x => new CategoryTreeDto(x.Id, x.Name, x.IsActive, Build(x.Id)))
                .ToList();

        return Build(null);
    }

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var dto = await _uow.Repository<Category>()
            .Query()
            .Where(x => x.Id == id)
            .Select(x => new CategoryDto(
                x.Id, x.Name, x.Description, x.IsActive,
                x.ParentCategoryId,
                x.ParentCategory != null ? x.ParentCategory.Name : null,
                x.Products.Count, x.CreatedAt))
            .FirstOrDefaultAsync(ct);

        return dto ?? throw new AppException("Kategorija nije pronadjena.", 404);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Category>();
        var parentId = request.ParentCategoryId is > 0 ? request.ParentCategoryId : null;

        if (await repo.ExistsAsync(x => x.Name == request.Name && x.ParentCategoryId == parentId, ct))
            throw new AppException("Kategorija sa istim nazivom vec postoji na tom nivou.");

        if (parentId.HasValue && !await repo.ExistsAsync(x => x.Id == parentId, ct))
            throw new AppException("Nadredjena kategorija ne postoji.");

        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ParentCategoryId = parentId,
            IsActive = true
        };

        await repo.AddAsync(category, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(category.Id, ct);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Category>();
        var parentId = request.ParentCategoryId is > 0 ? request.ParentCategoryId : null;

        var category = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Kategorija nije pronadjena.", 404);

        if (parentId == id)
            throw new AppException("Kategorija ne moze biti sama sebi nadredjena.");

        if (parentId.HasValue && await IsDescendantAsync(parentId.Value, id, ct))
            throw new AppException("Nadredjena kategorija ne moze biti podredjena ovoj kategoriji.");

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.ParentCategoryId = parentId;
        category.IsActive = request.IsActive;

        repo.Update(category);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Category>();

        var category = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException("Kategorija nije pronadjena.", 404);

        if (await _uow.Repository<Product>().ExistsAsync(x => x.CategoryId == id, ct))
            throw new AppException("Kategorija se ne moze obrisati jer sadrzi proizvode.");

        if (await repo.ExistsAsync(x => x.ParentCategoryId == id, ct))
            throw new AppException("Kategorija se ne moze obrisati jer sadrzi podkategorije.");

        repo.Remove(category);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<bool> IsDescendantAsync(int candidateParentId, int categoryId, CancellationToken ct)
    {
        var all = await _uow.Repository<Category>()
            .Query()
            .Select(x => new { x.Id, x.ParentCategoryId })
            .ToListAsync(ct);

        var current = all.FirstOrDefault(x => x.Id == candidateParentId);

        while (current?.ParentCategoryId is not null)
        {
            if (current.ParentCategoryId == categoryId)
                return true;

            current = all.FirstOrDefault(x => x.Id == current.ParentCategoryId);
        }

        return false;
    }
}
