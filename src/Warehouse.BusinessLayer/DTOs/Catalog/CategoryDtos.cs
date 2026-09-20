using Warehouse.BusinessLayer.Common;

namespace Warehouse.BusinessLayer.DTOs.Catalog;

public class CategoryFilterRequest : PagedRequest
{
    public int? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public record CategoryDto(
    int Id,
    string Name,
    string? Description,
    bool IsActive,
    int? ParentCategoryId,
    string? ParentCategoryName,
    int ProductCount,
    DateTime CreatedAt);

public record CategoryTreeDto(
    int Id,
    string Name,
    bool IsActive,
    List<CategoryTreeDto> Children);

public record CreateCategoryRequest(string Name, string? Description, int? ParentCategoryId);

public record UpdateCategoryRequest(string Name, string? Description, int? ParentCategoryId, bool IsActive);
