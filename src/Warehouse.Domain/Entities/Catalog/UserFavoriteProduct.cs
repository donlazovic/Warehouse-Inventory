using Warehouse.Domain.Entities.Identity;

namespace Warehouse.Domain.Entities.Catalog;

public class UserFavoriteProduct
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
